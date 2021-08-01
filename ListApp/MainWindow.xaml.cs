using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using System.Windows.Threading;

namespace ListApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string directory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chart");
        static string freezer = @"fridge.txt";
        static string order = @"order.txt";
        string locale;
        static string localeFile = @"locale.txt";
        static List<string> fridgeList = new List<string>();
        static List<string> orderList = new List<string>();
        static string[] s;
        static string cur;
        static int num;
        static bool delete = false;
        static string sync = @"C:\Users\Public\Chart";
        DispatcherTimer frogTimer;
        public MainWindow()
        {
            InitializeComponent();
            frogTimer = new DispatcherTimer();
            TimeSpan ts = new TimeSpan(0, 0, 2);
            frogTimer.Interval = ts;
            frogTimer.Tick += ChangeFrog;
        }

        /// <summary>
        /// Takes lists from files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Initialized(object sender, EventArgs e)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            if (!File.Exists(System.IO.Path.Combine(directory, freezer)))
            {
                using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(directory, freezer), false))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        sw.WriteLine("Имеющийся продукт {0}", i + 1);
                    }
                }
            }
            if (!File.Exists(System.IO.Path.Combine(directory, order)))
            {
                using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(directory, order), false))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        sw.WriteLine("Будущий продукт {0}", i + 1);
                    }
                }
            }

            // Sync to Public
            if (File.Exists(System.IO.Path.Combine(sync, order)))
            {
                using (StreamReader sr = new StreamReader(System.IO.Path.Combine(sync, order)))
                {
                    using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(directory, order)))
                    {
                        while (!sr.EndOfStream)
                        {
                            sw.WriteLine(sr.ReadLine());
                        }
                    }
                }
            }
            if (File.Exists(System.IO.Path.Combine(sync, freezer)))
            {
                using (StreamReader sr = new StreamReader(System.IO.Path.Combine(sync, freezer)))
                {
                    using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(directory, freezer)))
                    {
                        while (!sr.EndOfStream)
                        {
                            sw.WriteLine(sr.ReadLine());
                        }
                    }
                }
            }

            s = File.ReadAllLines(System.IO.Path.Combine(directory, freezer));
            for (int i = 0; i < s.Length; i++)
            {
                fridgeList.Add(s[i]);
            }
            s = File.ReadAllLines(System.IO.Path.Combine(directory, order));
            for (int i = 0; i < s.Length; i++)
            {
                orderList.Add(s[i]);
            }
            Update();
        }

        /// <summary>
        /// Shows picture for 2000 ms and then shows content;
        /// Reads and changes locale (if needed)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            frogTimer.Start();
        }

        /// <summary>
        /// Replaces 'Frog' image on startup with the program content
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeFrog(object sender, EventArgs e)
        {
            frogTimer.Stop();
            CurrentStateScroll.Visibility = Visibility.Visible;
            NeedScroll.Visibility = Visibility.Visible;
            clearCurrent.Visibility = Visibility.Visible;
            PrintCurrent.Visibility = Visibility.Visible;
            clearOrder.Visibility = Visibility.Visible;
            PrintOrder.Visibility = Visibility.Visible;
            currentAdd.Visibility = Visibility.Visible;
            currentAddButton.Visibility = Visibility.Visible;
            orderAdd.Visibility = Visibility.Visible;
            orderAddButton.Visibility = Visibility.Visible;
            FrogImage.Visibility = Visibility.Hidden;
            if (File.Exists(System.IO.Path.Combine(directory, localeFile)))
            {
                using (StreamReader sr = new StreamReader(System.IO.Path.Combine(directory, localeFile)))
                {
                    locale = sr.ReadLine();
                }
                ChangeLanguage(locale);
            }
            else
            {
                System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.CurrentUICulture;
                ChangeLanguage(ci.Name);
            }
        }

        private void currentAddButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentAction();
        }

        private void orderAddButton_Click(object sender, RoutedEventArgs e)
        {
            OrderAction();
        }

        private void currentAdd_KeyDown(object sender, KeyEventArgs e)
        
        {
            if (e.Key == Key.Enter)
            {
                CurrentAction();
            }
        }

        private void orderAdd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OrderAction();
            }
        }

        /// <summary>
        /// Adds one new item to sorted List using binary search
        /// </summary>
        /// <param name="a"></param>
        /// <param name="add"></param>
        private void AddToSortedList(ref List<string> a, string add)
        {
            int l, r, m;
            l = -1;
            r = a.Count;
            while (r - l > 1)
            {
                m = (r + l) / 2;
                if (string.Compare(a[m], add) < 0)
                {
                    l = m;
                }
                else
                {
                    r = m;
                }
            }
            a.Insert(r, add);
        }

        /// <summary>
        /// Adds new item to the Current list or moves the item to the Order list
        /// </summary>
        private void CurrentAction()
        {
            string curAdd = currentAdd.Text;
            currentAdd.Text = "";
            FindOrInsert(ref fridgeList, ref orderList, curAdd);
        }

        /// <summary>
        /// Adds new item to the Order list or moves the item to the Current list
        /// </summary>
        private void OrderAction()
        {
            string curAdd = orderAdd.Text;
            orderAdd.Text = "";
            FindOrInsert(ref orderList, ref fridgeList, curAdd);
        }

        /// <summary>
        /// Takes two lists with items and new item and tries to insert new item to the given list,
        /// or delete item,
        /// or move item to the other given list
        /// </summary>
        /// <param name="where">List the item should be inserted</param>
        /// <param name="other">List the item may be transfered to</param>
        private void FindOrInsert(ref List<string> where, ref List<string> other, string cur)
        {
        TryFind:
            if (!int.TryParse(cur, out num))
            {
                bool add = true;
                if (cur.Length > 0 && cur[cur.Length - 1] == '!' && delete == false)
                {
                    delete = true;
                    cur = cur.Remove(cur.Length - 1, 1);
                    goto TryFind;
                }
                else if (cur.Length > 0 && cur[cur.Length - 1] == '?' && delete == false)
                {
                    cur = cur.Remove(cur.Length - 1, 1);
                    if (int.TryParse(cur, out int index))
                    {
                        if (index > where.Count || index <= 0)
                        {
                            InvalidValue();
                        }
                        else
                        {
                            if (locale == "ru-RU")
                            {
                                MessageBox.Show(where[index - 1], "Поиск по номеру");
                            }
                            else
                            {
                                MessageBox.Show(where[index - 1], "Find by number");
                            }
                        }
                    }
                    add = false;
                }
                if (add && cur != "")
                {
                    cur = cur.ToLower();
                    if (Find(cur, ref where))
                    {
                        AddToSortedList(ref where, cur);
                    }
                }
                else if (add)
                {
                    InvalidValue();
                }
            }
            else
            {
                if (num > where.Count || num <= 0)
                {
                    InvalidValue();
                }
                else
                {
                    if (delete != true)
                    {
                        AddToSortedList(ref other, where[num - 1]);
                    }
                    where.RemoveAt(num - 1);
                    delete = false;
                }
            }
            delete = false;
            Update();
        }

        /// <summary>
        /// Shows MessageBox for invalid entered value.
        /// The function is used by BOTH CurrentAction() and OrderAction()
        /// </summary>
        private void InvalidValue()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.Name == "ru-RU")
            {
                MessageBox.Show("Неверно задано значение. Попробуйте ещё раз");
            }
            else
            {
                MessageBox.Show("Invalid value. Try again");
            }
        }

        /// <summary>
        /// Replaces all 'ё' chars with 'е' chars in Russian items
        /// </summary>
        /// <param name="s">Item string</param>
        /// <returns></returns>
        private string Replace(string s)
        {
            return s.Replace('ё', 'е');
        }

        /// <summary>
        /// Updates lists in UI
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < fridgeList.Count; i++)
            {
                fridgeList[i] = fridgeList[i].ToLower();
                fridgeList[i] = Replace(fridgeList[i]);
            }
            for (int i = 0; i < orderList.Count; i++)
            {
                orderList[i] = orderList[i].ToLower();
                orderList[i] = Replace(orderList[i]);
            }
            //fridgeList.Sort();
            //orderList.Sort();
            currentState.Text = "";
            for (int i = 0; i < fridgeList.Count; i++)
            {
                currentState.Text += Convert.ToString(i + 1);
                currentState.Text += ". ";
                currentState.Text += fridgeList[i];
                currentState.Text += "\n";
            }
            need.Text = "";
            for (int i = 0; i < orderList.Count; i++)
            {
                need.Text += Convert.ToString(i + 1);
                need.Text += ". ";
                need.Text += orderList[i];
                need.Text += "\n";
            }
            SaveFiles();
        }

        /// <summary>
        /// Saves lists and clears files from Temp directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveFiles();
            if (File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\fridge.txt")))
            {
                File.Delete(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\fridge.txt"));
            }
            if (File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\order.txt")))
            {
                File.Delete(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\order.txt"));
            }
            using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(directory, localeFile), false))
            {
                sw.WriteLine(locale);
            }
        }

        /// <summary>
        /// Clears Current list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearCurrent_Click(object sender, RoutedEventArgs e)
        {
            if (System.Globalization.CultureInfo.CurrentCulture.Name == "ru-RU")
            {
                if (MessageBox.Show("Вы уверены, что хотите очистить список продуктов, которые сейчас едите?", "Вы уверены?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    fridgeList.Clear();
                    Update();
                }
            }
            else
            {
                if (MessageBox.Show("Are you sure you want to clear the list of products you eat now?", "Are you sure?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    fridgeList.Clear();
                    Update();
                }
            }
        }

        /// <summary>
        /// Merges two sorted Lists
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        List<string> Merge(List<string> a, List<string> b)
        {
            List<string> res = new List<string>();
            int i = 0;
            int j = 0;
            while (i < a.Count && j < b.Count)
            {
                if (string.Compare(a[i], b[j]) < 0)
                {
                    res.Add(a[i++]);
                }
                else
                {
                    res.Add(b[j++]);
                }
            }
            while (i < a.Count)
            {
                res.Add(a[i++]);
            }
            while (j < b.Count)
            {
                res.Add(b[j++]);
            }
            return res;
        }

        /// <summary>
        /// Moves the Order list to the Current list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearOrder_Click(object sender, RoutedEventArgs e)
        {
            if (System.Globalization.CultureInfo.CurrentCulture.Name == "ru-RU")
            {
                if (MessageBox.Show("Вы уверены, что хотите очистить список покупок и перенести его элементы в список \"Едим сейчас\"?", "Вы уверены?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    fridgeList = Merge(fridgeList, orderList);
                    orderList.Clear();
                    Update();
                }
            }
            else
            {
                if (MessageBox.Show("Are you sure you want to clear the list of products to buy and move the items to the \"Stuff to use\" list?", "Are you sure?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    fridgeList = Merge(fridgeList, orderList);
                    orderList.Clear();
                    Update();
                }
            }
        }

        /// <summary>
        /// Opens default text editor with Current list.
        /// No changes saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintCurrent_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\fridge.txt")))
            {
                File.Delete(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\fridge.txt"));
            }
            File.Copy(System.IO.Path.Combine(directory, freezer), System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\fridge.txt"));
            Process.Start(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\fridge.txt"));
        }

        /// <summary>
        /// Opens default text editor with Order list. No changes saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintOrder_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\order.txt")))
            {
                File.Delete(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\order.txt"));
            }
            File.Copy(System.IO.Path.Combine(directory, order), System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\order.txt"));
            Process.Start(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\order.txt"));
        }

        /// <summary>
        /// Saves lists in text files in Local directory and in Public directory
        /// </summary>
        private void SaveFiles()
        {
            using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(directory, freezer), false))
            {
                for (int i = 0; i < fridgeList.Count; i++)
                {
                    sw.WriteLine(fridgeList[i]);
                }
            }
            using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(directory, order), false))
            {
                for (int i = 0; i < orderList.Count; i++)
                {
                    sw.WriteLine(orderList[i]);
                }
            }
            // Sync to Public
            if (!Directory.Exists(sync))
            {
                Directory.CreateDirectory(sync);
            }
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(directory, order)))
            {
                using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(sync, order)))
                {
                    while (!sr.EndOfStream)
                    {
                        sw.WriteLine(sr.ReadLine());
                    }
                }
            }
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(directory, freezer)))
            {
                using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(sync, freezer)))
                {
                    while (!sr.EndOfStream)
                    {
                        sw.WriteLine(sr.ReadLine());
                    }
                }
            }
        }

        /// <summary>
        /// Finds all items in collection which contain given string
        /// </summary>
        /// <param name="s"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        private bool Find(string s, ref List<string> collection)
        {
            List<string> found = new List<string>();
            string res = "";
            s = s.ToLower();
            s = Replace(s);
            List<string> sCol = s.Split().ToList();
            for (int i = 0; i < collection.Count; i++)
            {
                foreach (string item in sCol)
                {
                    if (collection[i].Contains(item) && (found.Count != 0 && found[found.Count - 1] != Convert.ToString(i + 1) || found.Count == 0))
                    {
                        res += Convert.ToString(i + 1);
                        res += "\n";
                        found.Add(Convert.ToString(i + 1));
                    }
                }
            }
            if (res != "")
            {
                string text;
                string caption;
                if (locale == "ru-RU")
                {
                    text = "У вас есть пункты, частично или полностью содержащие " + s + ":\n" + res + "Добавить " + s + " в список?";
                    caption = "Результаты поиска";
                }
                else
                {
                    text = "Some items include " + s + ":\n" + res + "Should I add " + s + " to the list anyway?";
                    caption = "Search results";
                }
                MessageBoxResult mr = MessageBox.Show(text, caption, MessageBoxButton.YesNo);
                if (mr == MessageBoxResult.Yes)
                {
                    return true;
                }
                else return false;
            }
            return true;
        }

        /// <summary>
        /// Handles EnglishVersionButton.Click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnglishVersionButton_Click(object sender, RoutedEventArgs e)
        {
            locale = "en-US";
            ChangeLanguage(locale);
        }

        /// <summary>
        /// Handles RussianVersionButton.Click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RussianVersionButton_Click(object sender, RoutedEventArgs e)
        {
            locale = "ru-RU";
            ChangeLanguage(locale);
        }

        /// <summary>
        /// Changes language
        /// </summary>
        /// <param name="locale"></param>
        private void ChangeLanguage(string locale)
        {
            if (locale == "ru-RU")
            {
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ru-RU");
                System.Threading.Thread.CurrentThread.CurrentCulture = ci;
                System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
                ClearFreezerText.Text = "Очистить\nсписок";
                PrintCurrentText.Text = "Версия\nдля\nпечати";
                ClearOrderText.Text = "Очистить\nсписок\nи перенести\nнаверх";
                PrintOrderText.Text = "Версия\nдля\nпечати";
                StuffToUseText.Text = "Едим сейчас (нужно еще - N,\nбольше не нужно - N!,\nне было раньше - наберите название)";
                StuffToBuyText.Text = "Будем есть (добавить - наберите название,\nперенести наверх - N,\nудалить - N!)";
                RussianVersionButton.IsEnabled = false;
                RussianVersionButton.Visibility = Visibility.Hidden;
                EnglishVersionButton.IsEnabled = true;
                EnglishVersionButton.Visibility = Visibility.Visible;
                Title = "Корзинка";
            }
            else if (locale == "en-US")
            {
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
                System.Threading.Thread.CurrentThread.CurrentCulture = ci;
                System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
                ClearFreezerText.Text = "Clear";
                PrintCurrentText.Text = "Print";
                ClearOrderText.Text = "Accept\nand\nclear";
                PrintOrderText.Text = "Print";
                StuffToUseText.Text = "Stuff to use\nrepeat - type the number,\nnever again - type the number, then \"!\",\nnever before - type the name";
                StuffToBuyText.Text = "Stuff to buy\nadd - type the name,\nmove to the first list - type N,\n remove permanently - N!";
                EnglishVersionButton.IsEnabled = false;
                EnglishVersionButton.Visibility = Visibility.Hidden;
                RussianVersionButton.IsEnabled = true;
                RussianVersionButton.Visibility = Visibility.Visible;
                Title = "Chart";
            }
            else if (locale == "en-UK")
            {
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-UK");
                System.Threading.Thread.CurrentThread.CurrentCulture = ci;
                System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
                ClearFreezerText.Text = "Clear";
                PrintCurrentText.Text = "Print";
                ClearOrderText.Text = "Accept\nand\nclear";
                PrintOrderText.Text = "Print";
                StuffToUseText.Text = "Stuff to use\nrepeat - type the number,\nnever again - type the number, then \"!\",\nnever before - type the name";
                StuffToBuyText.Text = "Stuff to buy\nadd - type the name,\n remove - type the number";
                EnglishVersionButton.IsEnabled = false;
                EnglishVersionButton.Visibility = Visibility.Hidden;
                RussianVersionButton.IsEnabled = true;
                RussianVersionButton.Visibility = Visibility.Visible;
                Title = "Chart";
            }
            else if (locale == "")
            {
                locale = "ru-RU";
                ChangeLanguage(locale);
            }
            //else
            //{
            //	MessageBox.Show("Internal error!\nPlease reinstall the application.\nYou are welcome to find fresh release here:\nhttps://pegor.keenetic.pro/Setup.msi");
            //	Environment.Exit(0);
            //}
            else { ChangeLanguage("en-US"); return; }
        }
    }
}
