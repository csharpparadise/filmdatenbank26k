namespace FilmDatenBank.Services;

public enum ToastTyp { Erfolg, Fehler, Info }

public record ToastNachricht(string Text, ToastTyp Typ, Guid Id);

public class ToastService
{
    public List<ToastNachricht> Toasts { get; } = [];
    public event Action? OnChange;

    public void Erfolg(string text) => Hinzufuegen(text, ToastTyp.Erfolg);
    public void Fehler(string text) => Hinzufuegen(text, ToastTyp.Fehler);
    public void Info(string text)   => Hinzufuegen(text, ToastTyp.Info);

    public void Entfernen(Guid id)
    {
        var toast = Toasts.FirstOrDefault(t => t.Id == id);
        if (toast is not null)
        {
            Toasts.Remove(toast);
            OnChange?.Invoke();
        }
    }

    private void Hinzufuegen(string text, ToastTyp typ)
    {
        Toasts.Add(new ToastNachricht(text, typ, Guid.NewGuid()));
        OnChange?.Invoke();
    }
}
