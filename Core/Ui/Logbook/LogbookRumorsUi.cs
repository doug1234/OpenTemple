using OpenTemple.Core.IO.SaveGames.UiState;

namespace OpenTemple.Core.Ui.Logbook;

public class LogbookRumorsUi
{

    [TempleDllLocation(0x10c47d34)]
    public bool IsVisible { get; set; }

    public void Show()
    {
        Stub.TODO();
    }

    [TempleDllLocation(0x10190570)]
    public void Hide()
    {
        Stub.TODO();
    }

    [TempleDllLocation(0x10190130)]
    public void Add(int rumorId)
    {
        throw new System.NotImplementedException();
    }

    [TempleDllLocation(0x10190190)]
    public void Reset()
    {
        Stub.TODO();
    }

    [TempleDllLocation(0x10190380)]
    public SavedLogbookRumorsUiState Save()
    {
        Stub.TODO();
        return new SavedLogbookRumorsUiState();
    }

    [TempleDllLocation(0x10190410)]
    public void Load(SavedLogbookRumorsUiState savedState)
    {
        Stub.TODO();
    }
}