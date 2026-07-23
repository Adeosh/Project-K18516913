const STORAGE_URL = import.meta.env.VITE_STORAGE_URL || 'http://localhost:9000/';

export const getImageUrl = (path?: string | null): string => {
    if (!path) return '';

    if (path.startsWith('http://') || path.startsWith('https://')) {
        return path;
    }

    const baseUrl = STORAGE_URL.replace(/\/+$/, '');
    const cleanPath = path.replace(/^\/+/, '');

    return `${baseUrl}/${cleanPath}`;
};