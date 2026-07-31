import { useState, useEffect, useRef } from 'react';
import type { FC } from 'react';

const MAP_CONTAINER_ID = "ymap-container";
const API_KEY = import.meta.env.VITE_YMAPS_API_KEY || '';

interface YMapEvent {
    get(key: 'coords'): [number, number];
}

interface YPlacemark {
    geometry: {
        setCoordinates(coords: [number, number]): void;
    };
}

interface YMap {
    geoObjects: {
        add(geoObject: YPlacemark): void;
    };
    events: {
        add(eventName: string, callback: (e: YMapEvent) => void): void;
    };
    destroy(): void;
}

declare global {
    interface Window {
        ymaps?: {
            ready(callback: () => void): void;
            Map: new (
                elementId: string,
                options: { center: number[]; zoom: number; controls: string[] }
            ) => YMap;
            Placemark: new (
                geometry: number[],
                properties: Record<string, unknown>,
                options: Record<string, unknown>
            ) => YPlacemark;
        };
    }
}

interface GeocoderResponse {
    response?: {
        GeoObjectCollection?: {
            featureMember?: Array<{
                GeoObject?: {
                    metaDataProperty?: {
                        GeocoderMetaData?: {
                            text?: string;
                        };
                    };
                };
            }>;
        };
    };
}

export interface LocationData {
    fullAddress: string;
    latitude: number;
    longitude: number;
}

interface AddressMapPickerProps {
    initialLocation: LocationData | null;
    onChange: (location: LocationData) => void;
}

export const AddressMapPicker: FC<AddressMapPickerProps> = ({ initialLocation, onChange }) => {
    const [addressText, setAddressText] = useState(initialLocation?.fullAddress || '');
    const [isFetching, setIsFetching] = useState(false);

    const mapRef = useRef<YMap | null>(null);
    const placemarkRef = useRef<YPlacemark | null>(null);

    const onChangeRef = useRef(onChange);
    useEffect(() => {
        onChangeRef.current = onChange;
    }, [onChange]);

    const initialLocationRef = useRef(initialLocation);

    useEffect(() => {
        let isMounted = true;

        const fetchAddress = async (lat: number, lon: number) => {
            if (!API_KEY) {
                console.error("VITE_YMAPS_API_KEY не задан в .env");
                return;
            }

            try {
                const response = await fetch(
                    `https://geocode-maps.yandex.ru/1.x/?apikey=${API_KEY}&format=json&geocode=${lon},${lat}&results=1`
                );

                const data = (await response.json()) as GeocoderResponse;

                const geoObject = data.response?.GeoObjectCollection?.featureMember?.[0]?.GeoObject;
                const address = geoObject?.metaDataProperty?.GeocoderMetaData?.text || "Адрес определен, но без названия улицы";

                if (isMounted) {
                    setAddressText(address);
                    setIsFetching(false);
                }

                onChangeRef.current({
                    fullAddress: address,
                    latitude: lat,
                    longitude: lon
                });
            } catch (err: unknown) {
                if (err instanceof Error) {
                    console.error("Ошибка HTTP геокодирования:", err.message);
                }
                if (isMounted) {
                    setAddressText("Ошибка при получении адреса");
                    setIsFetching(false);
                }
            }
        };

        const initMap = () => {
            if (!isMounted) return;
            const container = document.getElementById(MAP_CONTAINER_ID);

            if (!container || mapRef.current || !window.ymaps) return;

            const location = initialLocationRef.current;
            const initialCoords: [number, number] = location?.latitude && location?.longitude
                ? [location.latitude, location.longitude]
                : [55.751574, 37.573856];

            const map = new window.ymaps.Map(MAP_CONTAINER_ID, {
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

            map.events.add('click', (e: YMapEvent) => {
                const coords = e.get('coords');

                placemark.geometry.setCoordinates(coords);

                if (isMounted) {
                    setIsFetching(true);
                    setAddressText('Определяем точный адрес...');
                }

                void fetchAddress(coords[0], coords[1]);
            });
        };

        const loadYmaps = () => {
            if (!API_KEY) {
                console.error("VITE_YMAPS_API_KEY не задан в .env");
                return;
            }

            if (window.ymaps) {
                window.ymaps.ready(initMap);
                return;
            }

            const scriptId = 'yandex-maps-v2-script';
            let script = document.getElementById(scriptId) as HTMLScriptElement | null;

            if (!script) {
                script = document.createElement('script');
                script.id = scriptId;
                script.src = `https://api-maps.yandex.ru/2.1/?apikey=${API_KEY}&lang=ru_RU`;
                script.async = true;
                document.head.appendChild(script);
            }

            script.addEventListener('load', () => {
                if (isMounted && window.ymaps) {
                    window.ymaps.ready(initMap);
                }
            });
        };

        loadYmaps();

        return () => {
            isMounted = false;
            if (mapRef.current) {
                try {
                    mapRef.current.destroy();
                } catch (err: unknown) {
                    if (err instanceof Error) {
                        console.error('Ошибка при удалении карты:', err.message);
                    }
                }
                mapRef.current = null;
            }
        };
    }, []);

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
                    id={MAP_CONTAINER_ID}
                    className="w-full h-full cursor-crosshair"
                />
            </div>
        </div>
    );
};