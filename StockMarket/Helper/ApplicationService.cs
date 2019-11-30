using Prism.Events;

namespace StockMarket
{
    internal sealed class ApplicationService
    {
        private ApplicationService()
        {
        }

        private static readonly ApplicationService _instance = new ApplicationService();

        internal static ApplicationService Instance
        {
            get { return _instance; }
        }

        private IEventAggregator _eventAggregator;

        internal IEventAggregator EventAggregator
        {
            get
            {
                if (this._eventAggregator == null)
                    this._eventAggregator = new EventAggregator();

                return this._eventAggregator;
            }
        }
    }

    public class UserChangedEvent : PubSubEvent<User>
    {
    }
}
