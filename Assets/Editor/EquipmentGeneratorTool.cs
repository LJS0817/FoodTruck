using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EquipmentGeneratorTool
{
    [MenuItem("Tycoon/Tools/Generate Basic Equipments")]
    public static void GenerateBasicEquipments()
    {
        string parentFolder = "Assets/ScriptableObjects";
        string folderName = "Equipment";
        string path = $"{parentFolder}/{folderName}";

        if (!AssetDatabase.IsValidFolder(parentFolder))
        {
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        }
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        EquipmentType[] types = {
            EquipmentType.Grill,
            EquipmentType.Refrigerator,
            EquipmentType.Fryer,
            EquipmentType.Blender,
            EquipmentType.CuttingBoard
        };

        string[] names = {
            "녹슨 낡은 그릴",
            "문 덜컹이는 냉장고",
            "기름때 낀 튀김기",
            "요란한 중고 믹서기",
            "흠집 투성이 도마"
        };

        string[] descs = {
            "불꽃이 이리저리 튀는 낡은 그릴입니다. 고기가 익기는 하지만 시간이 오래 걸립니다.",
            "모터 소리가 요란한 구형 냉장고입니다. 온도를 간신히 유지합니다.",
            "언제 청소했는지 모를 구형 튀김기입니다. 가끔 기름이 튑니다.",
            "스위치를 누르면 트럭 전체가 진동하는 중고 믹서기입니다.",
            "칼자국이 너무 많아 세균 번식이 의심되는 낡은 나무 도마입니다."
        };

        ProcessType[][] processTypes = {
            new ProcessType[] { ProcessType.Bake },
            new ProcessType[] { ProcessType.Cool, ProcessType.Frozen },
            new ProcessType[] { ProcessType.Fry },
            new ProcessType[] { ProcessType.Blend },
            new ProcessType[] { ProcessType.Cut }
        };

        // 기본 제공되는 Square 스프라이트 로드 (Packages 경로 사용)
        string spritePath = "Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/Square.png";
        Sprite defaultSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        
        if (defaultSprite == null)
        {
            Debug.LogWarning($"[주의] '{spritePath}' 경로에서 기본 스프라이트를 찾지 못했습니다. 스프라이트가 비어있는 상태로 생성됩니다.");
        }

        for (int i = 0; i < types.Length; i++)
        {
            EquipmentData newEquip = ScriptableObject.CreateInstance<EquipmentData>();
            newEquip.type = types[i];
            newEquip.equipmentName = names[i];
            newEquip.description = descs[i];
            newEquip.equipmentSprite = defaultSprite; // 임시 스프라이트 할당
            newEquip.tier = 1;
            newEquip.price = 0; // 기본 지급이므로 0원
            newEquip.tradeInValue = 0;
            
            newEquip.supportedProcessTypes = new List<ProcessTypeEntry>();

            foreach(ProcessType pType in processTypes[i])
            {
                ProcessTypeEntry entry = new ProcessTypeEntry();
                entry.processType = pType;
                
                // 좋지 않은 성능 수치
                entry.timeMultiplier = 1.0f; // 시간 단축 없음
                entry.staminaMultiplier = 1.0f; // 체력 소모 절감 없음
                entry.qualityBonus = 0f; // 품질 보너스 없음
                entry.miniGameEaseBonus = 0f; // 미니게임 난이도 완화 없음

                // 레벨업 성장치도 미미하게 부여
                entry.timeMultiplierGrowth = 0.05f;
                entry.staminaMultiplierGrowth = 0.05f;
                entry.qualityBonusGrowth = 0.01f;
                entry.miniGameEaseBonusGrowth = 0.01f;

                newEquip.supportedProcessTypes.Add(entry);
            }

            string assetPath = $"{path}/Equip_Basic_{types[i]}.asset";
            AssetDatabase.CreateAsset(newEquip, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=green>[성공]</color> 기본 지급용 형편없는 스펙의 도구 5종이 Assets/ScriptableObjects/Equipment 폴더에 생성되었습니다!");
    }
}
