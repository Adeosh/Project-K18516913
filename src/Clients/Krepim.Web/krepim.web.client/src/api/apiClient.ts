import axios, { AxiosError } from 'axios';

export interface ProblemDetails {
    type?: string;
    title: string;
    status: number;
    detail?: string;
    instance?: string;
    errors?: Array<{ code: string; description: string; type: number }>;
}

export const apiClient = axios.create({
    baseURL: import.meta.env.VITE_API_GATEWAY_URL || '',
    headers: {
        'Content-Type': 'application/json',
    },
});

apiClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('krepim_token');
        if (token && config.headers) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

apiClient.interceptors.response.use(
    (response) => response,
    (error: AxiosError<ProblemDetails>) => {
        if (error.response) {
            const problem = error.response.data || {};
            const status = error.response.status;

            console.error(`[API Error ${status}] ${problem.title || 'Error'}: ${problem.detail || 'No details provided'}`);

            switch (status) {
                case 401:
                    console.warn('Сессия устарела. Перенаправление на вход...');
                    localStorage.removeItem('krepim_token');
                    window.location.href = '/login';
                    break;
                case 403:
                    console.error('Доступ запрещен (Недостаточно прав ролевой модели).');
                    break;
                case 400:
                    if (problem.errors) {
                        problem.errors.forEach(err => console.warn(`Валидация [${err.code}]: ${err.description}`));
                    }
                    break;
                case 409:
                    console.error('Конфликт состояния данных:', problem.detail);
                    break;
                case 500:
                    console.error('Критическая ошибка сервера. Обратитесь к администратору.');
                    break;
            }
            return Promise.reject(problem);
        }

        return Promise.reject({ title: 'Network Error', status: 0, detail: error.message } as ProblemDetails);
    }
);