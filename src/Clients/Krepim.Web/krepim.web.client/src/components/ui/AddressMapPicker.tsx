import { useState, useEffect, useRef } from 'react';
import type { FC } from 'react';

declare global {
    interface Window {
        ymaps: any;
    }
}

interface LocationData {
    fullAddress: string;
    latitude: number;
    longitude: number;
}

interface AddressMapPickerProps {
    initialLocation: LocationData | null;
    onChange: (location: LocationData) => void;
}

export const AddressMapPicker: FC<AddressMapPickerProps> = ({ initialLocation, onChange }) => {
    const mapContainerId = "ymap-container";
    const apiKey = '2a0190f0-d6b5-4110-8438-5ba0ab22e8dc';

    const [addressText, setAddressText] = useState(initialLocation?.fullAddress || '');
    const [isFetching, setIsFetching] = useState(false);

    const mapRef = useRef<any>(null);
    const placemarkRef = useRef<any>(null);

    useEffect(() => {
        let isMounted = true;

        const loadYmaps = () => {
            if (window.ymaps) {
                window.ymaps.ready(initMap);
                return;
            }

            const scriptId = 'yandex-maps-v2-script';
            let script = document.getElementById(scriptId) as HTMLScriptElement;

            if (!script) {
                script = document.createElement('script');
                script.id = scriptId;
                script.src = `https://api-maps.yandex.ru/2.1/?apikey=${apiKey}&lang=ru_RU`;
                script.async = true;
                document.head.appendChild(script);
            }

            script.addEventListener('load', () => {
                if (isMounted && window.ymaps) {
                    window.ymaps.ready(initMap);
                }
            });
        };

        const initMap = () => {
            if (!isMounted) return;
            const container = document.getElementById(mapContainerId);
            if (!container || mapRef.current) return;

            const initialCoords = initialLocation?.latitude && initialLocation?.longitude
                ? [initialLocation.latitude, initialLocation.longitude]
                : [55.751574, 37.573856];

            const map = new window.ymaps.Map(mapContainerId, {
                center: initialCoords,
                zoom: 13,
                controls: ['zoomControl', 'geolocationControl']
            });

            const placemark = new window.ymaps.Placemark(initialCoords, {}, {
                preset: 'islands#redDotIcon'
            });

            map.geoObjects.add(placemark);
            mapRef.current = map;
            placemarkRef.current = placemark;
            map.events.add('click', (e: any) => {
                const coords = e.get('coords');

                placemark.geometry.setCoordinates(coords);

                setIsFetching(true);
                setAddressText('Определяем точный адрес...');

                void fetchAddress(coords[0], coords[1]);
            });
        };

        loadYmaps();

        return () => {
            isMounted = false;
            if (mapRef.current) {
                try { mapRef.current.destroy(); } catch (e) { }
                mapRef.current = null;
            }
        };
    }, []);

    const fetchAddress = async (lat: number, lon: number) => {
        try {
            const response = await fetch(`https://geocode-maps.yandex.ru/1.x/?apikey=${apiKey}&format=json&geocode=${lon},${lat}&results=1`);
            const data = await response.json();

            const geoObject = data.response?.GeoObjectCollection?.featureMember?.[0]?.GeoObject;
            const address = geoObject?.metaDataProperty?.GeocoderMetaData?.text || "Адрес определен, но без названия улицы";

            setAddressText(address);
            setIsFetching(false);
            onChange({
                fullAddress: address,
                latitude: lat,
                longitude: lon
            });
        } catch (err) {
            console.error("Ошибка HTTP геокодирования:", err);
            setAddressText("Ошибка при получении адреса");
            setIsFetching(false);
        }
    };

    return (
        <div className="space-y-4">
            <div className={`p-4 rounded-xl border-2 transition-colors ${isFetching ? 'bg-accent/10 border-accent/30' : 'bg-surface border-border'} shadow-sm`}>
                <div className="text-xs text-text-muted font-bold uppercase mb-1">Выбранный адрес</div>
                <div className="text-text font-medium min-h-[24px] flex items-center gap-2">
                    {isFetching && <span className="w-4 h-4 border-2 border-accent border-t-transparent rounded-full animate-spin"></span>}
                    <span className={isFetching ? 'text-accent font-semibold' : ''}>
                        {addressText || 'Кликните на карту, чтобы выбрать адрес'}
                    </span>
                </div>
            </div>

            <div className="w-full h-[400px] rounded-xl overflow-hidden border border-border shadow-inner">
                <div
                    id={mapContainerId}
                    className="w-full h-full cursor-crosshair"
                />
            </div>
        </div>
    );
};