using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace Client.Services;

public interface ITranslationService
{
    string CurrentLang { get; }
    string this[string key] { get; }
    event Action? OnLanguageChanged;
    Task InitializeAsync();
    Task SetLanguageAsync(string lang);
    string GetDirection();
}

public class TranslationService : ITranslationService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly HttpClient _localHttp;

    public string CurrentLang { get; private set; } = "fr";
    private Dictionary<string, string> Translations { get; set; } = new();

    public event Action? OnLanguageChanged;

    public TranslationService(HttpClient http, IJSRuntime js, Microsoft.AspNetCore.Components.NavigationManager nav)
    {
        _http = http;
        _js = js;
        _localHttp = new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
    }

    public async Task InitializeAsync()
    {
        try
        {
            var savedLang = await _js.InvokeAsync<string>("localStorage.getItem", "app_lang");
            if (!string.IsNullOrEmpty(savedLang) && (savedLang == "fr" || savedLang == "ar"))
            {
                CurrentLang = savedLang;
            }
        }
        catch { /* Ignore if JS interop fails initially */ }

        await LoadTranslations();
    }

    public async Task SetLanguageAsync(string lang)
    {
        if (lang != "fr" && lang != "ar") return;

        CurrentLang = lang;
        try { await _js.InvokeVoidAsync("localStorage.setItem", "app_lang", lang); } catch { }
        
        await LoadTranslations();
        OnLanguageChanged?.Invoke();
    }

    private async Task LoadTranslations()
    {
        try
        {
            var json = await _localHttp.GetFromJsonAsync<Dictionary<string, string>>($"i18n/{CurrentLang}.json");
            if (json != null) Translations = json;
        }
        catch
        {
            Translations = new Dictionary<string, string>();
        }
    }

    public string this[string key]
    {
        get
        {
            if (Translations.TryGetValue(key, out var value))
                return value;
            
            return key; // return key if not found
        }
    }

    public string GetDirection() => CurrentLang == "ar" ? "rtl" : "ltr";
}
