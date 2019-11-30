using System.Windows;
using System.Windows.Controls;

namespace StockMarket
{
    /// <summary>
    /// A Usercontrol containing two TextBlocks in a vertical StackPanel.
    /// </summary>
    /// Interaktionslogik für ListStackPanelUser.xaml
    public partial class UserStackPanelList : UserControl
    {

        /// <summary>
        /// Gets or sets the Text of the upper TextBlock.
        /// </summary>
        public string HeaderText
        {
            get { return (string)this.GetValue(HeaderTextProperty); }
            set { this.SetValue(HeaderTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register("HeaderText", typeof(string), typeof(UserStackPanelList), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the Text of the lower TextBlock.
        /// </summary>
        public string ValueText
        {
            get { return (string)this.GetValue(ValueTextProperty); }
            set { this.SetValue(ValueTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register("ValueText", typeof(string), typeof(UserStackPanelList), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStackPanelList"/> class.
        /// </summary>
        public UserStackPanelList()
        {
            this.InitializeComponent();
        }
    }
}
