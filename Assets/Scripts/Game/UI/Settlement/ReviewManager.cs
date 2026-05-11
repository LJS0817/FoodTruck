using System.Collections.Generic;
using UnityEngine;

public static class ReviewManager
{
    private static readonly string[] goodReviews = {
        "음식이 입에서 녹아요! 최고! 😋",
        "웨이팅 한 보람이 있네요. JMT!",
        "사장님이 친절하고 음식이 맛있어요.",
        "인생 맛집 등극. 내일 또 올게요~",
        "오늘 먹은 메뉴 진짜 미쳤음 꼭 드세요 ㅠㅠ"
    };

    private static readonly string[] premiumReviews = {
        "우와, 오늘 요리 퀄리티가 미쳤어요! 완전 고급 레스토랑급 ✨",
        "프리미엄의 맛이 느껴집니다. 비싸도 돈값 하네요.",
        "정성이 듬뿍 들어간 느낌. 대접받는 기분이었습니다."
    };

    private static readonly string[] badReviews = {
        "줄이 너무 길어서 화가 납니다. 😡",
        "기다리다가 굶어 죽을 뻔... 다신 안 감",
        "웨이팅 시스템 개선좀 하세요 ㅡㅡ",
        "알바생이라도 쓰지 너무 느림."
    };

    private static readonly string[] topMenuReviews = {
        "역시 소문대로 {0}이(가) 제일 맛있네요!",
        "여기 오면 무조건 {0} 드세요. 두 번 드세요."
    };

    public static List<string> GenerateDailyReviews(int satisfiedCustomers, int lostCustomers, int premiumDishes, string topMenu)
    {
        List<string> generated = new List<string>();

        // 1. 잃어버린 손님이 많을 때 (화난 리뷰)
        if (lostCustomers > 5)
        {
            generated.Add(badReviews[Random.Range(0, badReviews.Length)]);
        }

        // 2. 프리미엄 요리가 많이 팔렸을 때
        if (premiumDishes >= 3)
        {
            generated.Add(premiumReviews[Random.Range(0, premiumReviews.Length)]);
        }

        // 3. 탑 메뉴 칭찬
        if (!string.IsNullOrEmpty(topMenu) && topMenu != "None")
        {
            string template = topMenuReviews[Random.Range(0, topMenuReviews.Length)];
            generated.Add(string.Format(template, topMenu));
        }

        // 4. 일반적인 좋은 리뷰 (빈자리 채우기, 1~2개)
        int fillCount = Mathf.Clamp(3 - generated.Count, 1, 3);
        for (int i = 0; i < fillCount; i++)
        {
            generated.Add(goodReviews[Random.Range(0, goodReviews.Length)]);
        }

        // 간단한 섞기 (Fisher-Yates)
        for (int i = 0; i < generated.Count; i++)
        {
            int r = Random.Range(i, generated.Count);
            string temp = generated[i];
            generated[i] = generated[r];
            generated[r] = temp;
        }

        return generated;
    }
}
