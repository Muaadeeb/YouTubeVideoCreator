using AnimeStoryVideoCreator.Client.Services.Interfaces;

namespace AnimeStoryVideoCreator.Client.Services;

public class BusyService : IBusyService
{
    private int _depth;
    private string _title = "Working…";
    private string _detail = "";
    private DateTime? _startedUtc;

    public bool IsBusy => _depth > 0;
    public string Title => _title;
    public string Detail => _detail;
    public DateTime? StartedUtc => _startedUtc;

    public event Action? Changed;

    public IDisposable Show(string title, string? detail = null)
    {
        _depth++;
        _title = title;
        _detail = detail ?? "";
        if (_depth == 1)
            _startedUtc = DateTime.UtcNow;
        Changed?.Invoke();
        return new Scope(this);
    }

    public void Update(string detail)
    {
        if (_depth <= 0) return;
        _detail = detail;
        Changed?.Invoke();
    }

    public void Hide()
    {
        if (_depth <= 0) return;
        _depth--;
        if (_depth <= 0)
        {
            _depth = 0;
            _title = "Working…";
            _detail = "";
            _startedUtc = null;
        }
        Changed?.Invoke();
    }

    private sealed class Scope : IDisposable
    {
        private BusyService? _owner;
        public Scope(BusyService owner) => _owner = owner;
        public void Dispose()
        {
            _owner?.Hide();
            _owner = null;
        }
    }
}
