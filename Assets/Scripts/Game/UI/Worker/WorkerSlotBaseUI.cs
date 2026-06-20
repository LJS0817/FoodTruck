using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkerSlotBaseUI : MonoBehaviour
{
    [Header("Base UI Elements")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text gradeText;
    public TMP_Text statText;           // 4대 기본 스탯 표시용

    public WorkerAbilityDescUI[] abilityIcons; // Array to hold ability icons
    public bool showAbilityDescriptions = false; // Toggle to show/hide ability descriptions
    public CanvasGroup AbilityDescriptionGroup; // For controlling visibility and interactivity
    public CanvasGroup StatGroup; // For controlling visibility and interactivity of stats

    protected WorkerData _currentWorker;
    protected WorkerManagementUI _parentUI;

    public void SetupBaseSlot(WorkerData worker)
    {
        _currentWorker = worker;

        if (AbilityDescriptionGroup != null)
        {
            AbilityDescriptionGroup.alpha = 0f;
            AbilityDescriptionGroup.interactable = false;
            AbilityDescriptionGroup.blocksRaycasts = false;
        }

        if (StatGroup != null)
        {
            StatGroup.alpha = 1f;
            StatGroup.interactable = true;
            StatGroup.blocksRaycasts = true;
        }

        if (nameText != null) nameText.text = $"{worker.workerName}";
        
        if (gradeText != null) 
        {
            gradeText.text = worker.grade.ToString() + "급";
            gradeText.color = GetGradeColor(worker.grade);
        }

        // 기본 스탯(능력치) 텍스트 세팅
        if (statText != null)
        {
            statText.text = $"요리 : {worker.cookSkill}\n" +
                            $"손님 응대 : {worker.humanSkill}\n" +
                            $"체력 : {worker.stamina}\n" +
                            $"청소 : {worker.cleanSkill}";
        }

        // 패시브 스킬(능력) 아이콘 및 텍스트 세팅
        if (worker.abilities == null || worker.abilities.Count == 0)
        {
            // 능력치 아이콘 모두 끄기
            foreach (WorkerAbilityDescUI ability in abilityIcons)
            {
                ability.SetAbilityInfo(null);
            }
        }
        else
        {
            // string desc = "";
            for (int i = 0; i < worker.abilities.Count; i++)
            {
                var abilityInstance = worker.abilities[i];
                float val = abilityInstance.baseValue + ((worker.currentLevel - 1) * 0.05f);
                int percent = Mathf.RoundToInt(val * 100);

                // 원본 ScriptableObject(WorkerAbilityData) 찾기
                WorkerAbilityData abilityData = null;
                if (WorkerManager.Instance != null && WorkerManager.Instance.availableAbilities != null)
                {
                    abilityData = WorkerManager.Instance.availableAbilities.Find(a => a.abilityType == abilityInstance.abilityType);
                }
                abilityIcons[i].SetAbilityInfo(abilityData, percent);

                // if (abilityData != null)
                // {
                //     // 능력치 이름과 증가 수치만 간결하게 표시
                //     desc += $"- {abilityData.abilityName}: +{percent}%\n";
                //     abilityIcons[i].SetAbilityInfo(abilityData);
                // }
                // else
                // {
                //     // 폴백 (데이터를 찾지 못한 경우)
                //     desc += $"- {GetAbilityName(abilityInstance.abilityType)}: +{percent}%\n";
                // }
            }

            for (int i = worker.abilities.Count; i < abilityIcons.Length; i++)
            {
                abilityIcons[i].SetAbilityInfo(null);
            }
        }
    }

    private Color GetGradeColor(WorkerGrade grade)
    {
        switch (grade)
        {
            case WorkerGrade.S: return new Color(1f, 0.8f, 0f); // Gold
            case WorkerGrade.A: return new Color(0.8f, 0.2f, 1f); // Purple
            case WorkerGrade.B: return new Color(0.2f, 0.6f, 1f); // Blue
            case WorkerGrade.C: return new Color(0.2f, 0.8f, 0.2f); // Green
            case WorkerGrade.D: return Color.white;
            case WorkerGrade.E: return Color.gray;
            case WorkerGrade.F: return new Color(0.3f, 0.3f, 0.3f); // Dark Gray
            default: return Color.white;
        }
    }

    public void ChangeSlotView()
    {
        showAbilityDescriptions = !showAbilityDescriptions;

        if (AbilityDescriptionGroup != null)
        {
            AbilityDescriptionGroup.alpha = showAbilityDescriptions ? 1f : 0f;
            AbilityDescriptionGroup.interactable = showAbilityDescriptions;
            AbilityDescriptionGroup.blocksRaycasts = showAbilityDescriptions;
        }

        if (StatGroup != null)
        {
            StatGroup.alpha = showAbilityDescriptions ? 0f : 1f;
            StatGroup.interactable = !showAbilityDescriptions;
            StatGroup.blocksRaycasts = !showAbilityDescriptions;
        }
    }
}
