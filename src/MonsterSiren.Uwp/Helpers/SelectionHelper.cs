namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为选择列表提供帮助方法。
/// </summary>
public sealed class SelectionHelper
{
    private readonly ListViewBase targetList;
    private readonly FlyoutBase selectionFlyout;
    private readonly FlyoutBase contextFlyout;
    private readonly Action<FlyoutBase> setCurrectFlyoutAction;

    /// <summary>
    /// 构造 <see cref="SelectionHelper"/> 的新实例。
    /// </summary>
    /// <param name="targetList">目标列表控件。</param>
    /// <param name="selectionFlyout">选择菜单。</param>
    /// <param name="contextFlyout">普通菜单。</param>
    /// <param name="setCurrectFlyoutAction">为 ViewModel 设置当前菜单的委托。</param>
    /// <exception cref="ArgumentNullException">参数为 <see langword="null"/>。</exception>
    public SelectionHelper(ListViewBase targetList, FlyoutBase selectionFlyout, FlyoutBase contextFlyout, Action<FlyoutBase> setCurrectFlyoutAction)
    {
        this.targetList = targetList ?? throw new ArgumentNullException(nameof(targetList));
        this.selectionFlyout = selectionFlyout ?? throw new ArgumentNullException(nameof(selectionFlyout));
        this.contextFlyout = contextFlyout ?? throw new ArgumentNullException(nameof(contextFlyout));
        this.setCurrectFlyoutAction = setCurrectFlyoutAction ?? throw new ArgumentNullException(nameof(setCurrectFlyoutAction));
    }

    /// <summary>
    /// 开始选择操作。
    /// </summary>
    /// <param name="selectedItem">操作开始时被选择的项目，用于将列表切换为选择模式时将此项目选中。</param>
    public void StartMultipleSelection(object selectedItem = null)
    {
        targetList.SelectionMode = ListViewSelectionMode.Multiple;
        if (selectedItem is not null)
        {
            targetList.SelectedItem = selectedItem;
        }
        targetList.IsItemClickEnabled = false;
        setCurrectFlyoutAction?.Invoke(selectionFlyout);
    }

    /// <summary>
    /// 停止选择操作。
    /// </summary>
    public void StopMultipleSelection()
    {
        targetList.SelectionMode = ListViewSelectionMode.None;
        targetList.IsItemClickEnabled = true;
        setCurrectFlyoutAction?.Invoke(contextFlyout);
    }

    /// <summary>
    /// 选择列表。
    /// </summary>
    /// <param name="maxRange">选择的最大值。</param>
    public void SelectList(int maxRange)
    {
        targetList.SelectRange(new ItemIndexRange(0, (uint)maxRange));
    }

    /// <summary>
    /// 取消选择列表。
    /// </summary>
    /// <param name="maxRange">选择的最大值。</param>
    public void DeselectList(int maxRange)
    {
        // TODO: 取消选择的方法存在先前选择项目残留的问题。
        targetList.DeselectRange(new ItemIndexRange(0, (uint)maxRange));
    }
}
