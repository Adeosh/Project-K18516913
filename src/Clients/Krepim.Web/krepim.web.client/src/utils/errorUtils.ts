export const extractErrorMessage = (err: unknown, defaultMessage: string = 'Произошла непредвиденная ошибка'): string => {
    if (err && typeof err === 'object') {
        const errorData = err as {
            response?: { data?: { detail?: string, title?: string } },
            data?: { detail?: string, title?: string },
            detail?: string,
            title?: string,
            message?: string
        };

        return errorData.response?.data?.detail
            || errorData.response?.data?.title
            || errorData.data?.detail
            || errorData.data?.title
            || errorData.detail
            || errorData.title
            || errorData.message
            || defaultMessage;
    }

    if (typeof err === 'string') {
        return err;
    }

    return defaultMessage;
};