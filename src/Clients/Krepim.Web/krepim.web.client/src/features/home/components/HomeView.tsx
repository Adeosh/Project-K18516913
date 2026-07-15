import { useState, useEffect } from 'react';
import type { FC } from 'react';
import { Link } from 'react-router-dom';
import { apiClient } from '@/api/apiClient';
import type { CategoryDto } from '../api/managerCatalogApi';

import bannerMain from '../../../assets/images/banners/banner_2.png';

import logoBolt from '../../../assets/images/partners/boltru-130x100.jpg';
import logoMetall from '../../../assets/images/partners/metallservice-130x100.jpg';
import logoMtk from '../../../assets/images/partners/mtk-130x100.jpg';
import logoPic from '../../../assets/images/partners/ooo-pic-130x100.jpg';
import logoRuss from '../../../assets/images/partners/russconnect-130x100.jpg';
import logoVolz from '../../../assets/images/partners/volzgskiy-instrument-130x100.jpg';
import logoZitar from '../../../assets/images/partners/zitar-130x100.jpg';

import iconAnker from '../../../assets/images/icons/general/anchors_2.png';
import iconBolts from '../../../assets/images/icons/general/bolts_2.png';
import iconArma from '../../../assets/images/icons/general/arma.png';
import iconDubel from '../../../assets/images/icons/general/dubel.png';
import iconGaiki from '../../../assets/images/icons/general/gaiki.png';
import iconGvozdi from '../../../assets/images/icons/general/nails_2.png';
import iconSelfRezi from '../../../assets/images/icons/general/selfr.png';
import iconTools from '../../../assets/images/icons/general/tools.png';

import iconGeoloc from '../../../assets/images/icons/general/geo.png';
import iconMoney from '../../../assets/images/icons/general/money_2.png';
import iconClock from '../../../assets/images/icons/general/clock_2.png';
import iconConsultation from '../../../assets/images/icons/general/cons.png';
import iconFastDeliv from '../../../assets/images/icons/general/fast_deliv.png';

const PARTNERS = [
    { name: 'Bolt ru', src: logoBolt },
    { name: 'МеталлСервис', src: logoMetall },
    { name: 'МТК', src: logoMtk },
    { name: 'Первая инструментальная', src: logoPic },
    { name: 'Rusconnect', src: logoRuss },
    { name: 'Волжский инструмент', src: logoVolz },
    { name: 'Zitar', src: logoZitar },
];

const FEATURES = [
    { icon: iconFastDeliv, title: 'Быстрая доставка', desc: 'в день заказа' },
    { icon: iconMoney, title: 'Низкие цены', desc: 'гибкая система скидок' },
    { icon: iconGeoloc, title: 'Пункт выдачи', desc: 'в Сосновом Бору' },
    { icon: iconConsultation, title: 'Консультации', desc: 'поможем с выбором' },
    { icon: iconClock, title: 'Работаем', desc: 'с 9 до 19 часов' },
];

const PROMO_CATEGORIES = [
    { src: iconAnker, title: 'Анкера' },
    { src: iconBolts, title: 'Болты' },
    { src: iconDubel, title: 'Дюбели' },
    { src: iconGaiki, title: 'Гайки' },
    { src: iconGvozdi, title: 'Гвозди' },
    { src: iconArma, title: 'Шурупы' },
    { src: iconSelfRezi, title: 'Саморезы' },
    { src: iconTools, title: 'Все категории' },
];

export const HomeView: FC = () => {
    const [dbCategories, setDbCategories] = useState<CategoryDto[]>([]);

    useEffect(() => {
        const fetchCategories = async () => {
            try {
                const response = await apiClient.get<any>('/api/products/categories');
                const data = response.data?.value || response.data;
                if (Array.isArray(data)) {
                    setDbCategories(data);
                }
            } catch (e) {
                console.error('Ошибка загрузки категорий на главной', e);
            }
        };
        void fetchCategories();
    }, []);

    const getCategoryLink = (title: string) => {
        const found = dbCategories.find(c =>
            c.name.toLowerCase().includes(title.toLowerCase())
        );

        if (found) {
            return `/catalog?categoryId=${found.id}`;
        }
        return `/catalog?search=${title}`;
    };

    return (
        <div className="flex flex-col gap-8 pb-8">
            <section className="w-full relative h-[300px] md:h-[450px]">
                <img src={bannerMain} className="w-full h-full object-cover" alt="Главный баннер" />
                <div className="absolute inset-0 bg-black/40 flex items-center px-8 md:px-16">
                    <div className="max-w-2xl text-white">
                        <h1 className="text-3xl md:text-5xl font-bold mb-4">Профессиональный крепеж</h1>
                        <p className="text-lg mb-6 opacity-90">Более 10 000 позиций на складе с доставкой в день заказа.</p>
                        <Link to="/register" className="bg-[#9E2F1F] px-6 py-3 rounded-lg font-bold hover:bg-[#85281a] transition">
                            Зарегистрироваться
                        </Link>
                    </div>
                </div>
            </section>

            <section className="px-4 md:px-8 max-w-7xl mx-auto w-full">
                <div className="bg-surface rounded-2xl shadow-sm border border-border/50 p-4 md:p-6">
                    <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-8 gap-3 md:gap-4">
                        {PROMO_CATEGORIES.map((cat, idx) => (
                            <Link
                                key={idx}
                                to={getCategoryLink(cat.title)}
                                className="block w-full overflow-hidden rounded-md border border-transparent hover:border-accent hover:scale-[1.03] hover:shadow-md transition-all duration-300"
                            >
                                <img
                                    src={cat.src}
                                    alt={cat.title}
                                    className="w-full h-auto object-contain rounded-md"
                                />
                            </Link>
                        ))}
                    </div>
                </div>
            </section>

            <section className="w-full py-6 border-y border-border/50 bg-surface overflow-hidden group">
                <div className="flex w-max animate-marquee gap-4 px-2 group-hover:[animation-play-state:paused]">
                    {[...PARTNERS, ...PARTNERS, ...PARTNERS, ...PARTNERS].map((p, index) => (
                        <div
                            key={`${p.name}-${index}`}
                            className="relative bg-surface p-4 h-20 w-36 flex items-center justify-center rounded-lg overflow-hidden border border-border/50 flex-shrink-0 cursor-pointer"
                        >
                            <div className="absolute inset-0 bg-[#9E2F1F] opacity-0 hover:opacity-5 transition-opacity duration-300" />
                            <img
                                src={p.src}
                                alt={p.name}
                                className="max-h-full max-w-full object-contain opacity-90 transition-transform duration-500 hover:scale-110 hover:opacity-100"
                            />
                        </div>
                    ))}
                </div>
            </section>

            <section className="px-6 py-12 bg-surface border-y border-border">
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-6 md:gap-8 justify-items-center">
                    {FEATURES.map((item, idx) => (
                        <div key={idx} className="flex flex-col items-center text-center gap-3">
                            <img
                                src={item.icon}
                                alt={item.title}
                                className="h-16 md:h-20 w-auto object-contain drop-shadow-sm hover:scale-110 transition-transform duration-300"
                            />
                            <div>
                                <div className="font-bold text-lg text-text">{item.title}</div>
                                <div className="text-text-muted text-sm">{item.desc}</div>
                            </div>
                        </div>
                    ))}
                </div>
            </section>

            <section className="px-6 max-w-5xl mx-auto space-y-6">
                <h2 className="text-3xl font-bold">Магазин Крепим.ПРО</h2>
                <p className="text-text-muted">Наша компания — это устойчивый и надежный партнер в сфере оптовой и розничной продажи метизов. Мы специализируемся на предоставлении высококачественных метизных изделий, удовлетворяя потребности как крупных промышленных предприятий, так и мелких бизнесов, а также индивидуальных клиентов.</p>

                <h3 className="text-xl font-bold">Наши преимущества:</h3>
                <ul className="list-disc pl-6 space-y-2 text-text-muted">
                    <li><b>Качество:</b> Мы работаем только с надёжными поставщиками и гарантируем высокое качество нашей продукции. Все метизы соответствуют стандартам и нормативам качества.</li>
                    <li><b>Оперативность:</b> Мы ценим ваше время и предоставляем оперативную обработку заказов и быструю доставку. Ваша продукция будет у вас вовремя.</li>
                    <li><b>Клиентоориентированность:</b> Мы гордимся высоким уровнем обслуживания клиентов и стремимся удовлетворить все ваши потребности. Наши специалисты всегда готовы помочь в выборе правильных метизов и консультировать вас.</li>
                    <li><b>Гибкие условия:</b> Мы предлагаем гибкие ценовые условия и скидки для постоянных клиентов, а также готовы рассмотреть индивидуальные соглашения. <b><br />Не важно, нужны вам метизы для строительства, ремонта или промышленных процессов — мы всегда готовы стать вашим надежным поставщиком. С нами вы получите доступ к широкому выбору высококачественных метизов и лучшему обслуживанию.</b></li>
                </ul>
            </section>
        </div>
    );
};