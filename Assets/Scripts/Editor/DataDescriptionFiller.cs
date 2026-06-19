using UnityEngine;
using UnityEditor;

public class DataDescriptionFiller
{
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        if (EditorPrefs.GetBool("DataDescFilled_V1", false)) return;
        EditorPrefs.SetBool("DataDescFilled_V1", true);

        // FoodData 업데이트
        string[] foodGuids = AssetDatabase.FindAssets("t:FoodData");
        int foodCount = 0;
        foreach (string guid in foodGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FoodData data = AssetDatabase.LoadAssetAtPath<FoodData>(path);
            if (data != null)
            {
                data.description = GetDescriptionForFood(data.name);
                EditorUtility.SetDirty(data);
                foodCount++;
            }
        }

        // IngredientData 업데이트
        string[] ingGuids = AssetDatabase.FindAssets("t:IngredientData");
        int ingCount = 0;
        foreach (string guid in ingGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            IngredientData data = AssetDatabase.LoadAssetAtPath<IngredientData>(path);
            if (data != null)
            {
                data.description = GetDescriptionForIngredient(data.name);
                EditorUtility.SetDirty(data);
                ingCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"<color=green>[DataDescriptionFiller] 푸드 {foodCount}개, 재료 {ingCount}개의 '꿀잼' 설명 업데이트 완료!</color>");
    }

    private static string GetDescriptionForFood(string name)
    {
        switch (name)
        {
            // Burger
            case "BasicBurger": return "빵, 고기, 양상추라는 완벽한 삼위일체! 자본주의 사회에서 가장 빠르고 저렴하게 칼로리를 채울 수 있는 혁명적인 발명품입니다. 가성비를 따지는 현대인들에게 영혼의 안식처가 되어줍니다.";
            case "BaconBurger": return "삼겹살을 바싹 구워 햄버거에 넣을 생각을 한 최초의 인류에게 노벨 평화상을! 짭짤하게 바스러지는 베이컨의 식감이 평범한 버거를 프리미엄으로 둔갑시키는 기적을 보여줍니다.";
            case "CheeseBurger": return "미각을 지배하는 꾸덕꾸덕한 노란색 마법! 치즈 한 장이 패티의 온기에 녹아내리는 순간, 이것은 단순한 햄버거가 아니라 혈관에 직접 꽂히는 합법적 마약이 됩니다.";
            case "ChickenBurger": return "닭가슴살 샐러드가 다이어트 식품이라면, 빵 사이에 끼운 치킨 패티는 정신 건강을 위한 다이어트 식품입니다. 바삭한 튀김옷과 촉촉한 육즙이 팍팍한 삶을 위로해 줍니다.";
            case "EggBurger": return "아침을 든든하게 시작하라는 어머니의 마음을 햄버거에 담았습니다. 반숙 계란이 터지며 흐르는 노른자는 그 어떤 소스보다 완벽한 풍미를 자랑합니다.";
            case "VeggieBurger": return "건강을 챙기고 싶지만 햄버거는 포기할 수 없는 모순적인 현대인들을 위한 최고의 타협안! 채소로 만들었으니 살이 찌지 않을 거라는 자기 최면을 걸기에 아주 적합합니다.";

            // Hotdog
            case "BasicHotdog": return "길거리 음식의 영원한 베스트셀러! 한 손에는 스마트폰을, 다른 한 손에는 핫도그를 쥔 바쁜 도시인들을 위한 완벽한 인체공학적 디자인의 간식입니다. 케첩과 머스터드 범벅은 필수죠.";
            case "BaconHotdog": return "고기로 만든 소시지를 또 고기로 감싸다니, 이 얼마나 자본주의적이고 아름다운 발상인가요? 한 입 베어 무는 순간 입안 가득 퍼지는 극강의 훈연 향에 지갑이 절로 열립니다.";
            case "CheeseHotdog": return "소시지가 보이지 않을 정도로 쏟아부은 체다 치즈 소스! 입가를 노랗게 물들이며 먹어야 제맛인 이 핫도그는, 먹는 내내 죄책감과 황홀함을 동시에 선사하는 길티 플레저의 끝판왕입니다.";
            case "SpicyHotdog": return "스트레스 받는 날, 상사에게 받은 분노를 혓바닥의 고통으로 승화시켜줄 화끈한 핫도그! 먹다 보면 땀이 맺히고 콧물이 흐르지만, 돌아서면 또 생각나는 묘한 중독성을 가졌습니다.";
            case "VeggieHotdog": return "'핫도그에서 고기 맛이 안 나면 무슨 소용이냐'는 편견을 깨부수는 상큼함! 야채가 가득해 먹고 나서도 속이 더부룩하지 않아, 사장님의 양심을 덜어주는 착한 메뉴입니다.";

            // Mexican
            case "BeefTaco": return "멕시코의 정열을 작은 토르티야 안에 욱여넣었습니다! 육즙 팡팡 터지는 소고기와 매콤상큼한 살사의 조합은, 손님들로 하여금 마라카스를 흔들며 춤추고 싶게 만듭니다.";
            case "ChickenTaco": return "가벼운 주머니 사정에도 멕시칸 감성을 느끼고 싶은 학생들을 위한 최고의 픽! 라임 즙을 쫙 뿌려 먹으면 이국적인 풍미가 폭발하여 잠시나마 휴양지에 온 듯한 착각을 불러일으킵니다.";
            case "PorkTaco": return "돼지고기의 풍부한 감칠맛과 쫄깃한 식감이 타코의 신세계를 엽니다. 한 입 먹으면 손가락 사이로 뚝뚝 떨어지는 육즙은 타코가 진정한 소울 푸드임을 증명합니다.";
            case "BreakfastBurrito": return "아침 식사할 시간조차 없는 현대인들에게 바치는 헌사! 계란, 감자, 고기를 은박지에 둘둘 말아낸 이 무기 하나면, 퇴근할 때까지 거뜬하게 버틸 수 있는 에너지가 충전됩니다.";
            case "SpicyBeefBurrito": return "화끈한 매운맛을 찾는 맵부심 가득한 손님들의 도전 정신을 자극하는 폭탄 부리토! 고기보다 혀끝을 타격하는 매운 소스가 주도권을 쥐고 흔듭니다. 우유를 미리 준비하세요!";
            case "VeggieBurrito": return "지구를 사랑하는 평화주의자들을 위한 가장 두툼하고 든든한 초록색 벽돌. 고기가 들어가지 않았는데도 포만감이 엄청나서 다이어터를 혼란에 빠뜨립니다.";

            // Noodle
            case "CheeseNoodle": return "면발 하나하나에 꾸덕한 치즈 코팅을 입혔습니다! 라면인지 치즈 탕인지 정체성이 모호해질 정도로 진한 풍미를 자랑하며, 우울증마저 단번에 치료할 것 같은 칼로리를 자랑합니다.";
            case "ChickenNoodle": return "아플 때 끓여먹던 영혼의 수프를 상업적으로 완벽하게 재현했습니다! 맑고 깊은 닭육수와 부드럽게 넘어가는 면발은 지친 심신을 따뜻하게 데워주는 마법의 물약과 같습니다.";
            case "ColdNoodle": return "찜통 같은 한여름, 에어컨보다 더 확실하게 체온을 낮춰주는 얼음 동동 냉 누들! 이가 시릴 정도로 차가운 육수 한 모금이면 세상 근심이 다 얼어붙어 사라지는 기적을 체험할 수 있습니다.";
            case "MeatNoodle": return "면은 거들 뿐, 진짜 목적은 산처럼 쌓인 고기 토핑에 있습니다! 젓가락으로 고기를 파헤쳐야 비로소 면이 모습을 드러내는, 육식주의자들의 열렬한 지지를 받는 궁극의 사치 누들입니다.";
            case "SpicyNoodle": return "매운맛으로 스트레스를 푸는 대한민국 현대인들의 필수 영양소! 입술이 퉁퉁 부어오르고 눈물이 핑 돌지만, 젓가락을 절대 멈출 수 없는 악마적인 중독성을 자랑합니다.";

            // RiceBowl
            case "BeefRiceBowl": return "단짠단짠 특제 소스를 머금은 소고기가 하얀 쌀밥을 부드럽게 감싸 안았습니다. 젓가락으로 훌훌 비벼 먹으면 5분 만에 그릇이 텅 비어버리는 마술을 경험하게 될 것입니다.";
            case "ChickenRiceBowl": return "치밥은 진리! 바삭하거나 촉촉한 닭고기를 감칠맛 넘치는 소스에 버무려 밥 위에 얹었습니다. 호불호가 절대 갈리지 않는, 남녀노소 누구나 지갑을 열게 만드는 마성의 덮밥입니다.";
            case "PorkRiceBowl": return "기름진 돼지고기의 육즙이 밥알 하나하나에 코팅되어 눈부신 윤기를 발산합니다. 한 숟갈 크게 떠서 입에 넣는 순간, 오늘은 다이어트를 포기해야겠다는 굳은 결심을 하게 만들어 줍니다.";
            case "SpicyPorkRiceBowl": return "스트레스 킬러 제육덮밥! 불향 가득 머금은 매콤한 돼지고기 볶음은 한국인이라면 DNA 레벨에서 반응할 수밖에 없는 소울 푸드입니다. 밥 한 공기로는 턱없이 부족할지도 모릅니다.";
            case "VeggieRiceBowl": return "건강검진 결과를 보고 충격받은 직장인들의 회개용 식단! 형형색색의 신선한 채소들이 밥 위에 만개하여, 먹는 순간 몸이 정화되고 내일 다시 고기를 먹을 수 있는 힘을 줍니다.";
            case "FriedRice": return "기름을 듬뿍 코팅해 밥알이 춤추듯 고슬고슬하게 볶아낸 정통 볶음밥! 웍질하는 셰프의 손목 건강과 맞바꾼 눈물겨운 감칠맛을 자랑합니다.";

            // Salad
            case "BaconEggSalad": return "풀만 먹기엔 너무 억울한 육식파들을 위한 타협점. 야채의 아삭함 사이로 훅 들어오는 짭짤한 베이컨과 든든한 달걀이, 이것은 절대 가벼운 다이어트식이 아님을 증명합니다.";
            case "ChickenSalad": return "피트니스 센터 회원들의 주식 1호! 퍽퍽한 닭가슴살의 한계를 맛있는 드레싱과 신선한 야채로 극복해낸 눈물겨운 노력의 결정체입니다. 근육이 펌핑되는 느낌을 혀끝으로 즐기세요.";
            case "GreenSalad": return "자연 그대로의 초록빛 에너지를 그릇에 담았습니다! 코끼리도 초식동물이라는 사실을 일깨워주듯, 먹다 보면 턱관절이 뻐근해질 정도로 풍부한 채소의 향연이 펼쳐집니다.";
            case "MeatSalad": return "이름만 샐러드일 뿐, 사실상 '풀을 곁들인 고기 모둠'에 가깝습니다. 다이어트 중이라는 핑계로 고기를 양껏 먹고 싶어 하는 현대인들의 은밀한 욕망을 완벽하게 충족시켜 줍니다.";
            case "PotatoSalad": return "마요네즈와 삶은 감자가 만들어내는 부드럽고 묵직한 하모니. 입안에서 솜사탕처럼 녹아내리지만, 칼로리는 절대 녹아내리지 않는다는 무서운 비밀을 간직한 마성의 샐러드입니다.";

            // Sandwich
            case "BLT": return "베이컨(B), 양상추(L), 토마토(T)라는 지구상에서 가장 완벽한 3단 콤보! 누가 발명했는지 모르지만 이 세 가지 조합은 인간의 미각을 극대화시키는 우주적인 비율을 자랑합니다.";
            case "ChickenSandwich": return "턱이 빠질 듯한 엄청난 두께의 치킨 패티를 빵 사이에 우겨넣었습니다! 한 입에 먹기 불가능에 가까워 체면 따위는 던져버리고 야성미 넘치게 뜯어먹어야 제맛인 와일드 샌드위치.";
            case "EggSandwich": return "폭신폭신한 빵 사이에 빈틈없이 채워 넣은 부드러운 에그 마요. 입술 주변에 다 묻히고 먹어도 기분 좋은 달콤함과 고소함이, 빡빡한 아침 출근길에 한 줄기 빛이 되어줍니다.";
            case "HamCheeseSandwich": return "햄과 치즈, 그 흔하디흔한 조합이 왜 수십 년간 샌드위치계의 제왕으로 군림하고 있는지 증명하는 클래식 메뉴입니다. 화려하진 않지만 언제 먹어도 배신하지 않는 든든한 친구죠.";
            case "Sandwich": return "모든 재료가 어우러져 만들어내는 교향곡 같은 오리지널 샌드위치! 냉장고에 있는 재료를 몽땅 털어 넣어 만들었지만, 왠지 모르게 고급 브런치 카페에서 파는 듯한 착각을 줍니다.";

            // Special
            case "CheeseFries": return "튀긴 감자만으로도 이미 죄악인데, 그 위에 꾸덕하고 진한 체다 치즈 소스를 폭포수처럼 쏟아부었습니다. 혈관이 비명을 지르지만 혓바닥은 기립박수를 치는 궁극의 간식입니다.";
            case "Omelet": return "버터의 풍미를 한껏 머금은 폭신하고 부드러운 달걀 이불! 그 안을 조심스레 가르면 치즈와 야채, 고기가 용암처럼 쏟아져 나오는 시각적 쾌감과 미각적 감동을 동시에 선사합니다.";
            case "Steak": return "그 어떤 수식어도 필요 없는 자본주의 맛의 결정체! 뜨거운 불판 위에서 치명적인 소리를 내며 구워지는 이 고기 덩어리는, 당신이 돈을 열심히 벌어야 하는 가장 명확한 이유를 알려줍니다.";
            case "StirFriedVegetables": return "센 불에 빠르게 볶아내어 채소의 아삭함을 살린 야채 볶음입니다. 철판의 불맛을 덧입은 야채의 반란이 시작되며, 고기 없이도 젓가락을 멈출 수 없게 만듭니다.";
            case "BreadTower": return "물리 법칙을 무시하고 식빵과 재료들을 바벨탑처럼 쌓아 올린 전설의 메뉴! 어떻게 먹어야 할지 감조차 오지 않는 압도적인 스케일로, SNS에 사진을 올리기 위한 최고의 아이템입니다.";

            default: return "사장님의 영혼과 트럭의 가스비가 듬뿍 들어간 미지의 메뉴입니다. 설명할 시간에 한 입이라도 더 드시는 걸 추천합니다!";
        }
    }

    private static string GetDescriptionForIngredient(string name)
    {
        switch (name)
        {
            // ETC
            case "BellPepper": return "빨갛고 노란 색감으로 요리의 비주얼을 캐리하는 파프리카! 매운맛이 날 것 같지만 의외로 달달해서 편식하는 아이들의 통수를 칩니다.";
            case "Bread": return "어떤 재료든 너그럽게 품어주는 포용력의 상징, 식빵! 바삭하게 굽는 순간 집안 가득 퍼지는 고소한 향기는 늦잠 자는 사람도 벌떡 일어나게 만듭니다.";
            case "BurgerBun": return "패티의 육즙을 듬뿍 빨아들일 준비가 된 폭신한 햄버거 번. 위에 뿌려진 참깨 몇 알이 고급스러움을 더해주는 치밀한 디자인을 자랑합니다.";
            case "Cheese": return "어떤 쓰레기 같은 요리도 심폐소생술로 살려낸다는 만능 마법의 노란 장판! 쭉 늘어나는 쾌감 하나로 모든 죄책감을 잊게 만듭니다.";
            case "Egg": return "프라이, 스크램블, 삶기까지 모든 요리의 화룡점정을 담당하는 완전식품. 노른자를 톡 터뜨려 먹을 때 느껴지는 희열은 돈으로 살 수 없습니다.";
            case "HotdogBun": return "크고 아름다운 소시지를 온몸으로 감싸 안기 위해 태어난 길쭉한 빵. 케첩이 손에 묻지 않도록 방어막 역할을 훌륭히 수행합니다.";
            case "Lettuce": return "버거와 샌드위치에 아삭함을 더해주는 생명수 같은 양상추! 고기를 아무리 먹어도 이걸 곁들였으니 다이어트 중이라는 정신 승리를 가능케 합니다.";
            case "Noodle": return "후루룩 빨아들이는 소리만으로도 식욕을 자극하는 면발! 끊지 않고 한 번에 흡입해야 진정한 고수로 인정받을 수 있는 마성의 탄수화물입니다.";
            case "Rice": return "탄수화물의 민족, 한국인의 혈관을 흐르는 에너지원! '밥 한 번 먹자'는 인사말에서 알 수 있듯 모든 인간관계의 시작이자 끝입니다.";
            case "TacoShell": return "바삭하게 튀겨져 U자 모양으로 굳어버린 불쌍하지만 맛있는 옥수수 쉘. 먹을 때마다 파사삭 부서지며 책상을 난장판으로 만드는 범인입니다.";
            case "Tortilla": return "모든 재료를 둥글게 말아 감싸버리는 멕시코의 마법 보자기. 뭐든 많이 넣을수록 맛있지만, 터지지 않게 마는 것은 온전히 요리사의 몫입니다.";

            // Meats
            case "Bacon": return "삼겹살을 연기에 그을려 짠맛을 극대화한, 자본주의가 낳은 최고의 식재료! 프라이팬에 굽는 소리만으로도 이웃집의 식욕을 테러할 수 있습니다.";
            case "Beef": return "가격표를 볼 때마다 손이 덜덜 떨리지만, 마블링을 보는 순간 지갑을 열게 되는 소고기! 핏기만 가시면 바로 입에 넣어야 하는 시간 싸움의 결정체입니다.";
            case "Chicken": return "치느님은 위대하다! 튀겨도, 구워도, 볶아도 맛있는 완전무결한 닭고기. 단백질을 보충한다는 훌륭한 핑계거리를 제공합니다.";
            case "Pork": return "기름기가 좔좔 흐르는 삼겹살부터 쫄깃한 목살까지, 서민들의 애환을 가장 많이 달래준 영혼의 고기! 타기 직전 바싹 구워야 제맛입니다.";
            case "Sausage": return "입술을 톡 치고 터지는 식감이 일품인 육즙 폭탄 소시지! 케첩과 머스터드라는 영혼의 단짝을 만나면 전투력이 무한대로 상승합니다.";

            // Sauce
            case "HotSauce": return "입술이 퉁퉁 붓고 혀가 얼얼해져도 자꾸만 뿌리게 되는 마성의 빨간 물약. 매운맛으로 삶의 고통을 잊고 싶은 날 필수 아이템입니다.";
            case "Ketchup": return "새콤달콤함으로 감자튀김의 존재 가치를 증명해 주는 토마토의 희생. 짜먹을 때 나는 민망한 소리마저 용서되는 국민 소스입니다.";
            case "Mayonnaise": return "계란과 기름을 섞어 만든 극강의 고소함! 어디에 뿌려도 훌륭하지만 칼로리를 생각하는 순간 손이 멈칫하게 되는 악마의 소스입니다.";
            case "Mustard": return "노란색의 톡 쏘는 매력으로 케첩의 영원한 라이벌이자 단짝. 핫도그 위에 지그재그로 뿌리는 순간 요리사의 예술적 감각이 폭발합니다.";

            // Vegetables
            case "Cabbage": return "돈가스 옆에서 산더미처럼 쌓인 채 묵묵히 제 몫을 다하는 든든한 조력자. 채 썰어 마요네즈와 케첩을 섞어 먹으면 그것이 바로 추억의 맛입니다.";
            case "Carrot": return "화려한 주황색으로 요리의 색감을 담당하지만, 편식러들에겐 영원한 기피 대상 1호! 하지만 카레에 없으면 왠지 섭섭한 존재입니다.";
            case "Cucumber": return "시원함과 아삭함을 동시에 가진 수분 폭탄! 호불호가 극명하게 갈려, 주문 시 '오이 빼주세요'라는 요청을 수없이 받게 만드는 문제의 채소입니다.";
            case "Onion": return "썰 때마다 요리사의 눈물샘을 자극하는 매운맛의 대명사. 하지만 열을 가해 볶는 순간, 세상 그 어떤 설탕보다 달콤해지는 츤데레 매력을 가졌습니다.";
            case "Potato": return "튀기거나, 삶거나, 으깨거나, 굽거나, 어떻게 조리해도 완벽한 폼을 보여주는 식재료계의 올라운드 플레이어. 감튀가 없는 햄버거는 상상할 수 없습니다!";
            case "Tomato": return "채소인지 과일인지 여전히 정체성 혼란을 겪고 있지만, 상큼한 맛 하나로 전 세계 요리를 점령한 붉은 제왕. 익혀 먹을수록 더 맛있어집니다.";

            default: return "어디서 굴러들어 온 지 모르는 미지의 재료입니다. 일단 냄새를 맡아보고 요리에 넣어보세요!";
        }
    }
}
