using UnityEngine;
using UnityEditor;
using System.IO;

public class TruckGeneratorTool
{
    [MenuItem("Tycoon/Tools/Generate 10 Trucks")]
    public static void GenerateAndAssignTrucks()
    {
        string parentFolder = "Assets/ScriptableObjects";
        string folderName = "Trucks";
        string path = $"{parentFolder}/{folderName}";

        // 디렉토리 확인 및 생성
        if (!AssetDatabase.IsValidFolder(parentFolder))
        {
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        }
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        // 10가지 다채로운 트럭 테마 데이터
        string[] truckNames = {
            "낡은 손수레", 
            "클래식 스낵 밴", 
            "빈티지 카페 트럭", 
            "텍사스 바베큐 트레일러", 
            "씨푸드 아쿠아 트럭", 
            "길거리 닌자 스시 카트", 
            "비건 오가닉 픽업트럭", 
            "네온 펑크 푸드 버스", 
            "파티용 더블데커", 
            "마스터 셰프 킹 카트"
        };

        string[] truckDescs = {
            "할머니가 물려주신 아주 작은 손수레입니다. 좁지만 정감이 갑니다.",
            "동네 골목에서 흔히 볼 수 있는 소형 스낵 밴입니다. 가성비가 훌륭합니다.",
            "레트로한 감성을 자극하는 커피&디저트 전용 트럭입니다.",
            "고기를 굽기 위한 최적의 환기 시스템과 그릴을 구비한 거대한 트레일러입니다.",
            "시원한 파도 느낌의 데칼이 들어간 트럭. 생선 요리에 제격입니다.",
            "야시장에서 가장 눈에 띄는 은밀하고 빠른 조리에 특화된 카트입니다.",
            "친환경 태양광 패널을 탑재해 연료 효율이 끝내주는 픽업트럭입니다.",
            "화려한 네온사인으로 무장한 미래지향적 푸드 버스. 전력 소모가 큽니다.",
            "2층에 손님이 앉을 수 있는 웨이팅 존이 결합된 초대형 파티 버스입니다.",
            "푸드트럭계의 황제! 넓은 내부와 압도적인 스펙을 자랑합니다."
        };

        int[] prices = { 0, 5000, 15000, 35000, 45000, 60000, 85000, 120000, 200000, 500000 };
        float[] fuels = { 50f, 100f, 120f, 180f, 150f, 140f, 300f, 200f, 250f, 500f }; // 연료(오가닉은 연비 높음)
        float[] gens = { 30f, 60f, 80f, 150f, 120f, 100f, 90f, 300f, 400f, 600f }; // 발전기(네온이나 더블데커는 높음)
        int[] layoutSizes = { 6, 6, 8, 8, 8, 10, 10, 10, 12, 12 }; // 최소 6칸, 최대 12칸, 짝수로 트럭마다 다양하게

        TruckData firstTruck = null;

        for (int i = 0; i < 10; i++)
        {
            TruckData newTruck = ScriptableObject.CreateInstance<TruckData>();
            newTruck.truckName = truckNames[i];
            newTruck.truckDescription = truckDescs[i];
            newTruck.purchasePrice = prices[i];
            newTruck.fuelCapacity = fuels[i];
            newTruck.generatorCapacity = gens[i];
            
            // 컨셉에 맞춰 내부 레이아웃 슬롯 크기를 다르게 할당
            newTruck.stationLayout = new EquipmentData[layoutSizes[i]]; 

            // 파일 이름은 영문으로 저장하여 안전성 도모
            string assetPath = $"{path}/Truck_{i+1:00}.asset";
            AssetDatabase.CreateAsset(newTruck, assetPath);

            if (i == 0) firstTruck = newTruck;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 씬에 있는 Truck 컴포넌트를 찾아 첫 번째 트럭 데이터 자동 연결
        Truck sceneTruck = Object.FindFirstObjectByType<Truck>();
        if (sceneTruck != null && firstTruck != null)
        {
            Undo.RecordObject(sceneTruck, "Assign First Truck Data");
            sceneTruck.truckData = firstTruck;
            EditorUtility.SetDirty(sceneTruck);
            Debug.Log($"<color=green>[성공]</color> 10개의 다채로운 테마 트럭 데이터를 생성하고, 씬의 '{sceneTruck.gameObject.name}'에 '{firstTruck.truckName}'을 자동 연결했습니다!");
        }
        else
        {
            Debug.LogWarning("[주의] 트럭 데이터는 10개 생성되었으나, 현재 열려있는 씬에서 'Truck' 컴포넌트를 찾지 못해 자동 연결은 스킵되었습니다.");
        }
    }
}
