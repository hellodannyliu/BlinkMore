using System.ComponentModel;
using System.Globalization;

namespace BlinkMore.Core;

public sealed class LocalizationService : INotifyPropertyChanged
{
    public LocalizationService(AppLanguage language)
    {
        Language = language;
    }

    public AppLanguage Language { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string this[string key] => TextCatalog.Get(Language, key);

    public string Format(string key, params object[] args)
        => string.Format(CultureInfo.InvariantCulture, this[key], args);

    public void SetLanguage(AppLanguage language)
    {
        if (Language == language)
            return;

        Language = language;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }
}
