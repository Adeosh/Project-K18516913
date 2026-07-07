import type { FC } from 'react';

export const Footer: FC = () => {
    return (
        <footer className="bg-surface border-t border-border mt-auto py-12">
            <div className="container mx-auto px-6 flex flex-wrap justify-center gap-12 md:gap-24 text-sm">

                <div className="flex flex-col min-w-[200px]">
                    <h4 className="font-bold mb-4 uppercase tracking-wider text-text">Информация</h4>
                    <ul className="space-y-2 text-text-muted">
                        <li><a href="/about" className="hover:text-accent transition-colors">О нас</a></li>
                        <li><a href="/delivery" className="hover:text-accent transition-colors">Доставка</a></li>
                        <li><a href="/privacy" className="hover:text-accent transition-colors">Политика конфиденциальности</a></li>
                    </ul>
                </div>

                <div className="flex flex-col min-w-[200px]">
                    <h4 className="font-bold mb-4 uppercase tracking-wider text-text">Контакты</h4>
                    <a href="tel:+79110074045" className="text-accent font-bold text-lg hover:underline transition-all">
                        +7 (911) 007-40-45
                    </a>
                    <a href="mailto:krepim.pro@mail.ru" className="text-text-muted mt-2 hover:text-accent transition-colors underline decoration-dotted">
                        krepim.pro@mail.ru
                    </a>
                    <div className="mt-4 text-text-muted">
                        <p>пн-пт: 09:00 - 19:00</p>
                        <p>сб: 09:00 - 18:00</p>
                        <p>вс: выходной</p>
                    </div>
                </div>

                <div className="flex flex-col min-w-[200px] max-w-[250px]">
                    <h4 className="font-bold mb-4 uppercase tracking-wider text-text">Адрес</h4>
                    <a
                        href="https://yandex.ru/maps/?pt=29.093637,59.873029&z=17&l=map"
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-text-muted hover:text-accent transition-all leading-snug"
                    >
                        Россия, Ленинградская область, Сосновый Бор, Ракопежское ш., 32
                    </a>
                </div>
            </div>

            <div className="text-center mt-12 pt-6 border-t border-border text-xs text-text-muted">
                © {new Date().getFullYear()} Крепим.ПРО. Сайт носит информационный характер.
            </div>
        </footer>
    );
};