using ImageCompareUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageCompareUI.View
{
    /// <summary>
    /// Interaction logic for ComparisonView.xaml
    /// </summary>
    public partial class ComparisonView : UserControl
    {
        private readonly InputBindingCollection _bindings = new();

        public ComparisonView()
        {
            InitializeComponent();
            Focusable = true;
            Loaded += (s, e) => Keyboard.Focus(this);

            var viewModel = new ComparisonViewModel();

            AddBinding(viewModel.DeleteCommand, Key.Left, ModifierKeys.None, 1);
            AddBinding(viewModel.DeleteCommand, Key.Right, ModifierKeys.None, 2);
            AddBinding(viewModel.SkipCommand, Key.S, ModifierKeys.None);

            DataContext = viewModel;
        }

        private KeyBinding AddBinding(ICommand command, Key key, ModifierKeys modifier, object? commandParameter = null)
        {
            var keyBinding = new KeyBinding
            {
                Command = command,
                CommandParameter = commandParameter,
                Key = key
            };
            if (modifier != ModifierKeys.None)
            {
                keyBinding.Modifiers = modifier;
            }

            InputBindings.Add(keyBinding);
            return keyBinding;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ((ComparisonViewModel)DataContext).CompareCommand.Execute(null);
            }
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Dispatcher.BeginInvoke(new Action(() => tb.SelectAll()));
            _bindings.AddRange(InputBindings);
            InputBindings.Clear();
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            InputBindings.AddRange(_bindings);
            _bindings.Clear();
        }
    }
}
