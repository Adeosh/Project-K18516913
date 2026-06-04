import type { FC } from 'react';
import { useState, useEffect } from 'react';
import { YMaps, Map, Placemark } from '@pbe/react-yandex-maps';

interface AddressLocation {
    fullAddress: string;
    latitude: number;
    longitude: number;
}

interface AddressMapPickerProps {
    initialLocation?: AddressLocation | null;
    onChange: (location: AddressLocation) => void;
}

export const AddressMapPicker: FC<AddressMapPickerProps> = ({ initialLocation, onChange }) => {
    const defaultCenter = [55.751574, 37.573856];

    const [coords, setCoords] = useState<number[]>(
        initialLocation?.latitude && initialLocation?.longitude
            ? [initialLocation.latitude, initialLocation.longitude]
            : defaultCenter
    );
    const [addressText, setAddressText] = useState(initialLocation?.fullAddress || '');
    const [isLoadingText, setIsLoadingText] = useState(false);
    const [ymaps, setYmaps] = useState<any>(null);

    useEffect(() => {
        if (initialLocation?.latitude && initialLocation?.longitude) {
            setCoords([initialLocation.latitude, initialLocation.longitude]);
            setAddressText(initialLocation.fullAddress);
        }
    }, [initialLocation?.latitude, initialLocation?.longitude, initialLocation?.fullAddress]);

    const fetchAddress = async (coordinates: number[], ymapsApi: any) => {
        setIsLoadingText(true);
        try {
            const response = await ymapsApi.geocode(coordinates, { provider: 'yandex#map', results: 1 });
            const firstGeoObject = response.geoObjects.get(0);

            if (firstGeoObject) {
                const address = firstGeoObject.getAddressLine();
                setAddressText(address);

                onChange({
                    fullAddress: address,
                    latitude: coordinates[0],
                    longitude: coordinates[1]
                });
            } else {
                setAddressText('Не удалось определить адрес');
            }
        } catch (error) {
            console.error('Ошибка геокодирования:', error);
            setAddressText('Ошибка службы адресов');
        } finally {
            setIsLoadingText(false);
        }
    };

    const handleMapClick = (e: any) => {
        const newCoords = e.get('coords');
        setCoords(newCoords);
        if (ymaps) {
            void fetchAddress(newCoords, ymaps);
        }
    };

    return (
        <div className="space-y-4">
            <div className="p-4 bg-bg border-2 border-border rounded-xl">
                <div className="text-xs text-text-muted font-bold uppercase mb-1">Определенный адрес</div>
                <div className="text-text font-medium min-h-[24px]">
                    {isLoadingText ? <span className="animate-pulse">Определяем...</span> : (addressText || 'Кликните на карту, чтобы выбрать адрес')}
                </div>
            </div>
            <div className="w-full h-[400px] rounded-xl overflow-hidden border border-border shadow-inner">
                <YMaps query={{ apikey: '859f6a95-3fee-4491-844b-b6800bee80cb', load: 'package.full', lang: 'ru_RU' }}>
                    <Map
                        state={{ center: coords, zoom: 14 }}
                        width="100%"
                        height="100%"
                        onClick={handleMapClick}
                        onLoad={(api) => setYmaps(api)}
                        modules={['geocode']}
                    >
                        <Placemark geometry={coords} options={{ preset: 'islands#redDotIcon' }} />
                    </Map>
                </YMaps>
            </div>
        </div>
    );
};