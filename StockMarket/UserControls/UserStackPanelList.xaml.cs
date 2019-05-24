using System.Windows;
using System.Windows.Controls;

namespace StockMarket
{
    /// <summary>
    /// A Usercontrol containing two TextBlocks in a vertical StackPanel
    /// </summary>
    /// Interaktionslogik für ListStackPanelUser.xaml
    public partial class UserStackPanelList : UserControl
    {

        /// <summary>
        /// The Text of the upper TextBlock
        /// </summary>
        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register("HeaderText", typeof(string), typeof(UserStackPanelList), new PropertyMetadata(string.Empty));


        /// <summary>
        /// The Text of the lower TextBlock
        /// </summary>
        public string ValueText
        {
            get { return (string)GetValue(ValueTextProperty); }
            set { SetValue(ValueTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register("ValueText", typeof(string), typeof(UserStackPanelList), new PropertyMetadata(string.Empty));




        public UserStackPanelList()
        {
            InitializeComponent();
        }
    }
}
