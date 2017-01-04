using System;
using System.Collections.Generic;
using System.IO;
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
using GwentTracker.Model;
using ServiceStack;
using W3SavegameEditor.Core.Savegame;
using W3SavegameEditor.Core.Savegame.Values;
using W3SavegameEditor.Core.Savegame.Variables;
using ServiceStack.Text;

namespace GwentTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var cards = File.ReadAllText(@"data\cards.csv").FromCsv<List<Card>>();
            var saveGame = SavegameFile.Read(@"data\test.sav");
            var cardCollection = ((BsVariable)saveGame.Variables[11]).Variables
                                                                     .Skip(2)
                                                                     .TakeWhile(v => v.Name != "SBSelectedDeckIndex")
                                                                     .Where(v => v.Name == "cardIndex" || v.Name == "numCopies")
                                                                     .ToArray();
            for (var i = 0; i < cardCollection.Length; i += 2)
            {
                var index = ((VariableValue<int>)((VlVariable)cardCollection[i]).Value).Value;
                var copies = ((VariableValue<int>)((VlVariable)cardCollection[i + 1]).Value).Value;
                var card = cards.SingleOrDefault(c => c.Index == index);

                if (card != null)
                {
                    card.Copies = copies;
                    card.Obtained = true;
                }
            }

            dataGrid.ItemsSource = cards;
        }
    }
}
