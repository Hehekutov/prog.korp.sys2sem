using Prac4.Models;

namespace Prac4.Data;

public static class DbInitializer
{
    public static void Seed(TourGuideDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var cities = new List<City>
        {
            new()
            {
                Name = "Уфа",
                Region = "Республика Башкортостан",
                Population = 1_144_000,
                Established = "1574 год",
                Climate = "континентальный",
                AccentColor = "#2f7d63",
                ImagePath = "https://commons.wikimedia.org/wiki/Special:FilePath/Ufa_Sunset.jpg",
                PhotoSourceUrl = "https://commons.wikimedia.org/wiki/File:Ufa_Sunset.jpg",
                CoatOfArmsPath = "https://commons.wikimedia.org/wiki/Special:FilePath/Coat_of_arms_of_Ufa.svg",
                CoatOfArmsSourceUrl = "https://commons.wikimedia.org/wiki/File:Coat_of_arms_of_Ufa.svg",
                HeroCaption = "Зеленый город на высоких берегах Белой",
                History = "Уфа выросла из крепости на месте слияния Белой и Уфы и стала крупным культурным, промышленным и научным центром Южного Урала. Город стоит на высоких берегах, поэтому многие прогулочные маршруты связаны с видовыми площадками, набережными и зелеными склонами. В Уфе заметно переплетаются башкирские традиции, купеческая история, советская архитектура и современная городская жизнь.",
                Attractions = new List<Attraction>
                {
                    new()
                    {
                        Name = "Памятник Салавату Юлаеву",
                        ShortDescription = "Один из главных символов Уфы, установленный на высоком берегу реки Белой.",
                        History = "Монумент открыли в 1967 году на высоком берегу Белой. Это крупнейшая конная статуя в России и один из самых узнаваемых символов Башкортостана. От площадки рядом с памятником хорошо видны река, Конгресс-холл Торатау и южная часть Уфы. Место особенно красиво вечером, когда панорама города подсвечивается закатным светом.",
                        ImagePath = "https://commons.wikimedia.org/wiki/Special:FilePath/Salavat_Yulaev_Panorama.jpg",
                        PhotoSourceUrl = "https://commons.wikimedia.org/wiki/File:Salavat_Yulaev_Panorama.jpg",
                        OpeningHours = "круглосуточно",
                        TicketPrice = "бесплатно",
                        Address = "Уфа, площадь Салавата Юлаева",
                        MapUrl = "https://yandex.ru/maps/?text=Уфа%2C%20площадь%20Салавата%20Юлаева"
                    },
                    new()
                    {
                        Name = "Мечеть Ляля-Тюльпан",
                        ShortDescription = "Современная мечеть с минаретами, напоминающими раскрытые бутоны тюльпана.",
                        History = "Комплекс построен в 1998 году. Два высоких минарета напоминают бутоны тюльпана, поэтому здание легко узнается даже издалека. Мечеть служит религиозным, образовательным и культурным центром. Ее часто включают в обзорные маршруты по северной части города благодаря необычной архитектуре и спокойной территории вокруг.",
                        ImagePath = "https://commons.wikimedia.org/wiki/Special:FilePath/Lala_Tulpan_(March_2025).jpg",
                        PhotoSourceUrl = "https://commons.wikimedia.org/wiki/File:Lala_Tulpan_(March_2025).jpg",
                        OpeningHours = "09:00-18:00",
                        TicketPrice = "бесплатно",
                        Address = "Уфа, ул. Комарова, 5",
                        MapUrl = "https://yandex.ru/maps/?text=Уфа%2C%20улица%20Комарова%2C%205"
                    },
                    new()
                    {
                        Name = "Горсовет",
                        ShortDescription = "Район у здания городской администрации и одна из узнаваемых точек центральной части Уфы.",
                        History = "Горсовет в Уфе обычно связывают с районом городской администрации на проспекте Октября. Это деловая часть города, рядом с которой проходят важные городские маршруты, расположены остановки общественного транспорта, скверы и административные здания. Место удобно включить в прогулку по современному центру Уфы: отсюда легко добраться до парков, торговых улиц и других городских объектов.",
                        ImagePath = "https://commons.wikimedia.org/wiki/Special:FilePath/Ufa%20city%20hall%202022-05%20street.jpg",
                        PhotoSourceUrl = "https://commons.wikimedia.org/wiki/File:Ufa_city_hall_2022-05_street.jpg",
                        OpeningHours = "здание снаружи доступно для осмотра круглосуточно",
                        TicketPrice = "бесплатно",
                        Address = "Уфа, проспект Октября, 120",
                        MapUrl = "https://yandex.ru/maps/?text=Уфа%2C%20проспект%20Октября%2C%20120"
                    }
                }
            },
            new()
            {
                Name = "Хабаровск",
                Region = "Хабаровский край",
                Population = 617_000,
                Established = "1858 год",
                Climate = "муссонный",
                AccentColor = "#1f6f8b",
                ImagePath = "https://commons.wikimedia.org/wiki/Special:FilePath/Amur_river%2C_Khabarovsk%2C_Russia_-_panoramio.jpg",
                PhotoSourceUrl = "https://commons.wikimedia.org/wiki/File:Amur_river,_Khabarovsk,_Russia_-_panoramio.jpg",
                CoatOfArmsPath = "https://commons.wikimedia.org/wiki/Special:FilePath/Coat_of_arms_of_Khabarovsk.svg",
                CoatOfArmsSourceUrl = "https://commons.wikimedia.org/wiki/File:Coat_of_arms_of_Khabarovsk.svg",
                HeroCaption = "Дальневосточный город с широкой амурской перспективой",
                History = "Хабаровск возник как военный пост на Амуре и постепенно стал административным, транспортным и культурным центром Дальнего Востока. В городе много широких улиц, видовых точек и прогулочных мест у воды. Амурская набережная, старые здания центра и близость к границе формируют характер Хабаровска как открытого дальневосточного города.",
                Attractions = new List<Attraction>
                {
                    new()
                    {
                        Name = "Утес на Амуре",
                        ShortDescription = "Историческая смотровая площадка с видом на Амур и городскую набережную.",
                        History = "Утес на Амуре расположен рядом с городской набережной и считается одной из главных видовых точек Хабаровска. Это место связано с ранней историей города и хорошо подходит для прогулки: отсюда открывается вид на Амур, речной вокзал, набережную и мостовые перспективы. Летом здесь особенно оживленно, а вечером удобно наблюдать закат над рекой.",
                        ImagePath = "https://commons.wikimedia.org/wiki/Special:FilePath/Khabarovsk_2024-08_110.jpg",
                        PhotoSourceUrl = "https://commons.wikimedia.org/wiki/File:Khabarovsk_2024-08_110.jpg",
                        OpeningHours = "круглосуточно",
                        TicketPrice = "бесплатно",
                        Address = "Хабаровск, ул. Шевченко, район Амурского утеса",
                        MapUrl = "https://yandex.ru/maps/?text=Хабаровск%2C%20Амурский%20утес"
                    },
                    new()
                    {
                        Name = "Спасо-Преображенский собор",
                        ShortDescription = "Высокий кафедральный собор, формирующий силуэт центральной части города.",
                        History = "Собор построен в начале XXI века и быстро стал важной архитектурной точкой Хабаровска. Высокие стены и золотые купола хорошо видны с набережной, площади Славы и центральных улиц. Внутри проходят богослужения, а рядом удобно продолжить маршрут к смотровым площадкам и прогулочным зонам у Амура.",
                        ImagePath = "https://commons.wikimedia.org/wiki/Special:FilePath/Cathedral_of_the_Transfiguration_(Khabarovsk).jpg",
                        PhotoSourceUrl = "https://commons.wikimedia.org/wiki/File:Cathedral_of_the_Transfiguration_(Khabarovsk).jpg",
                        OpeningHours = "08:00-19:00",
                        TicketPrice = "бесплатно",
                        Address = "Хабаровск, ул. Тургенева, 24",
                        MapUrl = "https://yandex.ru/maps/?text=Хабаровск%2C%20улица%20Тургенева%2C%2024"
                    },
                    new()
                    {
                        Name = "Парк Динамо",
                        ShortDescription = "Городской парк в центре Хабаровска с прудами, фонтанами, дорожками и зонами отдыха.",
                        History = "Парк Динамо расположен в центральной части Хабаровска и считается одним из удобных мест для прогулок. На территории есть городские пруды, фонтаны, дорожки, зеленые зоны, кафе, аттракционы и площадки для отдыха. Рядом находятся музыкальный театр и оживленные городские улицы, поэтому парк часто используют как спокойную паузу во время прогулки по центру.",
                        ImagePath = "https://commons.wikimedia.org/wiki/Special:FilePath/Верхний%20пруд%20Хабаровск%20и%20Платинум-Арена.JPG",
                        PhotoSourceUrl = "https://commons.wikimedia.org/wiki/File:Верхний_пруд_Хабаровск_и_Платинум-Арена.JPG",
                        OpeningHours = "круглосуточно",
                        TicketPrice = "бесплатно",
                        Address = "Хабаровск, ул. Карла Маркса, 62",
                        MapUrl = "https://yandex.ru/maps/?text=Хабаровск%2C%20улица%20Карла%20Маркса%2C%2062%2C%20парк%20Динамо"
                    }
                }
            },
            new()
            {
                Name = "Москва",
                Region = "Москва",
                Population = 13_100_000,
                Established = "1147 год",
                Climate = "умеренно-континентальный",
                AccentColor = "#b83c3c",
                ImagePath = "https://commons.wikimedia.org/wiki/Special:FilePath/St._Basil_and_Kremlin_wall.jpg",
                PhotoSourceUrl = "https://commons.wikimedia.org/wiki/File:St._Basil_and_Kremlin_wall.jpg",
                CoatOfArmsPath = "https://commons.wikimedia.org/wiki/Special:FilePath/Coat_of_arms_of_Moscow.svg",
                CoatOfArmsSourceUrl = "https://commons.wikimedia.org/wiki/File:Coat_of_arms_of_Moscow.svg",
                HeroCaption = "Столица с историческим центром и мощным городским ритмом",
                History = "Москва впервые упоминается в летописи в 1147 году. За века город стал политическим, экономическим и культурным центром страны. В историческом ядре Кремль, Красная площадь, соборы и старые торговые ряды соседствуют с музеями, парками, театрами, деловыми кварталами и современной транспортной системой.",
                Attractions = new List<Attraction>
                {
                    new()
                    {
                        Name = "Московский Кремль и Красная площадь",
                        ShortDescription = "Историческое сердце столицы: кремлевские стены, башни, соборы и главная площадь страны.",
                        History = "Московский Кремль сформировался как укрепленный центр города и на протяжении веков был связан с ключевыми событиями российской истории. Красная площадь расположена у восточной стены Кремля и объединяет несколько важных символов Москвы: Спасскую башню, собор Василия Блаженного, ГУМ и исторические фасады. Это место удобно смотреть пешком, выделив время на площадь, Александровский сад и виды с Большого Москворецкого моста.",
                        ImagePath = "https://commons.wikimedia.org/wiki/Special:FilePath/St._Basil_and_Kremlin_wall.jpg",
                        PhotoSourceUrl = "https://commons.wikimedia.org/wiki/File:St._Basil_and_Kremlin_wall.jpg",
                        OpeningHours = "круглосуточно, возможны ограничения во время мероприятий",
                        TicketPrice = "бесплатно; музеи Кремля оплачиваются отдельно",
                        Address = "Москва, Красная площадь",
                        MapUrl = "https://yandex.ru/maps/?text=Москва%2C%20Красная%20площадь"
                    },
                    new()
                    {
                        Name = "ВДНХ",
                        ShortDescription = "Большой выставочный и прогулочный комплекс с павильонами, фонтанами и музеями.",
                        History = "Выставка открылась в 1939 году и со временем превратилась в большой общественный парк с историческими павильонами, фонтанами, музеями и прогулочными аллеями. На территории можно совместить архитектурный маршрут, посещение павильонов, кафе и прогулку к Останкинскому парку. Вечером центральная часть ВДНХ выглядит особенно эффектно благодаря подсветке.",
                        ImagePath = "https://commons.wikimedia.org/wiki/Special:FilePath/Moscow%2C_VDNKh%2C_Central_Pavilion_and_Friendship_of_Nations_fountain_at_night_(10656622456).jpg",
                        PhotoSourceUrl = "https://commons.wikimedia.org/wiki/File:Moscow,_VDNKh,_Central_Pavilion_and_Friendship_of_Nations_fountain_at_night_(10656622456).jpg",
                        OpeningHours = "территория 24/7, павильоны по расписанию",
                        TicketPrice = "вход на территорию бесплатный",
                        Address = "Москва, проспект Мира, 119",
                        MapUrl = "https://yandex.ru/maps/?text=Москва%2C%20проспект%20Мира%2C%20119"
                    },
                    new()
                    {
                        Name = "Парк Победы",
                        ShortDescription = "Мемориальный парк на Поклонной горе с аллеями, фонтанами, музеем и монументом Победы.",
                        History = "Парк Победы на Поклонной горе был открыт к 50-летию Победы в Великой Отечественной войне. Центральная часть комплекса связана с Монументом Победы, площадью Победителей и Музеем Победы. Здесь можно гулять по широким аллеям, смотреть фонтаны, посещать музейные экспозиции и рассматривать военную технику под открытым небом. Это одно из главных мемориальных пространств Москвы.",
                        ImagePath = "https://commons.wikimedia.org/wiki/Special:FilePath/6727%20-%20Moscow%20-%20Poklonnaya%20Hill.JPG",
                        PhotoSourceUrl = "https://commons.wikimedia.org/wiki/File:6727_-_Moscow_-_Poklonnaya_Hill.JPG",
                        OpeningHours = "территория открыта круглосуточно, музеи по расписанию",
                        TicketPrice = "вход в парк бесплатный, музей оплачивается отдельно",
                        Address = "Москва, площадь Победы, 3",
                        MapUrl = "https://yandex.ru/maps/?text=Москва%2C%20площадь%20Победы%2C%203%2C%20Парк%20Победы"
                    }
                }
            }
        };

        context.Cities.AddRange(cities);
        context.SaveChanges();
    }
}
