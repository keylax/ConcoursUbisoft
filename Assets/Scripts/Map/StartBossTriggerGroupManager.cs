public class StartBossTriggerGroupManager : TriggerGroupManager
{
    public override void TriggerIsPressed(bool isPressed)
    {
        UpdateTriggersPressedCount(isPressed);
        if (_triggersPressedCount != _triggerManagersCount) return;

        gameObject.SetActive(false);

        StaticObjects.MapCreationController.LaunchBoss();
    }
}
