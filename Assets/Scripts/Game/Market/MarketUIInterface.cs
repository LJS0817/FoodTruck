using UnityEngine;

public interface MarketUIInterface
{
    public void OpenUI();
    public void CloseUI();
    public void ChangeCategory(int categoryIndex);
    public void SetVisibleCategory(int categoryIndex, bool isActive);
}