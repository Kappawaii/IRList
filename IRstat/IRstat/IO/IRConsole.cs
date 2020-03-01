using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace IRStat.Core
{
    /// <summary>
    /// Console intégrée à l'interface de IRStat
    /// </summary>
    class IRConsole
    {
        readonly TextBlock output;

        public IRConsole(TextBlock output)
        {
            //null check
            this.output = output ?? throw new ArgumentNullException(nameof(output));
        }

        /// <summary>
        /// Vide l'irConsole
        /// </summary>
        public void Clear() => output.Inlines.Clear();

        /// <summary>
        /// Affiche un message d'une couleur noire sans retour à la ligne
        /// </summary>
        /// <param name="text"></param>
        public void Print(string text) => Print(text, Brushes.Black);

        /// <summary>
        /// Affiche un message d'une couleur noire suivi d'un retour à la ligne
        /// </summary>
        /// <param name="text"></param>
        public void Println(string text) => Println(text, Brushes.Black);

        /// <summary>
        /// Affiche un message d'une couleur orange suivi d'un retour à la ligne
        /// </summary>
        /// <param name="text"></param>
        public void PrintlnWarning(string text) => Println(text, Brushes.Orange);

        /// <summary>
        ///  Affiche un message d'une couleur rouge sans retour à la ligne
        /// </summary>
        /// <param name="text"></param>
        public void PrintError(string text) => Print(text, Brushes.Red);

        /// <summary>
        /// Affiche un message d'une couleur rouge suivi d'un retour à la ligne
        /// </summary>
        /// <param name="text"></param>
        public void PrintlnError(string text) => Println(text, Brushes.Red);


        public void Println(object obj)
        {
            if (obj != null)
            {
                Println(obj.ToString(), Brushes.Blue);
            }
            else
            {
                Console.WriteLine("Null value passed to IRConsole, skipping");
                return;
            }
        }

        public void Println(Exception e)
        {
            if (e != null)
            {
                Println(e.Message, Brushes.Red);
            }
            else
            {
                Console.WriteLine("Null value passed to IRConsole, skipping");
                return;
            }
        }

        private void Println(string text, SolidColorBrush brush)
        {
            string temp = DateTime.Now.ToLongTimeString();
            temp = "[" + temp + "] ";
            Print(temp + text + "\n", brush);
        }

        private void Print(string text, SolidColorBrush brush)
        {
            Print(new Run(text) { Foreground = brush });
        }

        /// <summary>
        /// raccourci pour Println(Exception e);
        /// </summary>
        /// <param name="e"></param>
        public void PrintlnError(Exception e)
        {
            Println(e);
        }

        /// <summary>
        /// Commande de base pour afficher un Run dans l'IRConsole
        /// </summary>
        /// <param name="run"></param>
        private void Print(Run run)
        {
            output.Inlines.Add(run);
        }
    }
}