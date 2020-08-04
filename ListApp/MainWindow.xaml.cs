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
using System.IO;
using System.Windows.Shell;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Security.Policy;

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
		static List<string> f = new List<string>();
		static List<string> o = new List<string>();
		static string[] s;
		static string cur;
		static int num;
		static bool delete = false;
		static string sync = @"C:\Users\Public\Chart";
		public MainWindow()
		{
			InitializeComponent();
		}

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
				f.Add(s[i]);
			}
			s = File.ReadAllLines(System.IO.Path.Combine(directory, order));
			for (int i = 0; i < s.Length; i++)
			{
				o.Add(s[i]);
			}
			Update();
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			System.Threading.Thread.Sleep(2000);
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

		private void CurrentAction()
		{
			cur = currentAdd.Text;
			currentAdd.Text = "";
		A:
			if (!int.TryParse(cur, out num))
			{
				if (cur.Length > 0 && cur[cur.Length - 1] == '!' && delete == false)
				{
					delete = true;
					cur = cur.Remove(cur.Length - 1, 1);
					goto A;
				}
				if (cur != "")
				{
					cur = cur.ToLower();
					//isInList = false;
					//for (int i = 0; i < f.Count; i++)
					//{
					//	if (f[i] == cur)
					//	{
					//		isInList = true;
					//	}
					//}
					//if (!isInList)
					//{
					//	f.Add(cur);
					//}
					//else
					//{
					//	MessageBox.Show("У Вас уже есть " + cur + " в списке");
					//}
					//isInList = false;
					if (Find(cur, f))
					{
						f.Add(cur);
					}
				}
			}
			else
			{
				try
				{
					if (delete != true)
					{
						o.Add(f[num - 1]);
					}
					f.RemoveAt(num - 1);
					delete = false;
				}
				catch (Exception)
				{
					MessageBox.Show("Неверно задано значение. Попробуйте ещё раз");
				}
			}
			delete = false;
			Update();
		}

		private void OrderAction()
		{
			cur = orderAdd.Text;
			orderAdd.Text = "";
			if (!int.TryParse(cur, out num))
			{
				if (cur != "")
				{
					//isInList = false;
					//for (int i = 0; i < o.Count; i++)
					//{
					//	if (o[i] == cur)
					//	{
					//		isInList = true;
					//	}
					//}
					//if (!isInList)
					//{
					//	o.Add(cur);
					//}
					//else
					//{
					//	MessageBox.Show("У Вас уже есть " + cur + " в списке");
					//}
					//isInList = false;
					cur = cur.ToLower();
					if (Find(cur, o))
					{
						o.Add(cur);
					}
				}
			}
			else
			{
				try
				{
					o.RemoveAt(num - 1);
				}
				catch (Exception)
				{
					MessageBox.Show("Неверно задано значение. Попробуйте ещё раз");
				}
			}
			Update();
		}

		private string Replace(string s)
        {
			return s.Replace('ё', 'е');
        }

		private void Update()
		{
			for (int i = 0; i < f.Count; i++)
			{
				f[i] = f[i].ToLower();
				f[i] = Replace(f[i]);
			}
			for (int i = 0; i < o.Count; i++)
			{
				o[i] = o[i].ToLower();
				o[i] = Replace(o[i]);
			}
			f.Sort();
			o.Sort();
			currentState.Text = "";
			for (int i = 0; i < f.Count; i++)
			{
				currentState.Text += Convert.ToString(i + 1);
				currentState.Text += ". ";
				currentState.Text += f[i];
				currentState.Text += "\n";
			}
			need.Text = "";
			for (int i = 0; i < o.Count; i++)
			{
				need.Text += Convert.ToString(i + 1);
				need.Text += ". ";
				need.Text += o[i];
				need.Text += "\n";
			}
			SaveFiles();
		}

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

		private void clearCurrent_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Вы уверены, что хотите очистить список продуктов, которые сейчас едите?", "Вы уверены?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				f.Clear();
				Update();
			}
		}

		private void clearOrder_Click(object sender, RoutedEventArgs e)
		{
			f.AddRange(o);
			o.Clear();
			Update();
		}

		private void PrintCurrent_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\fridge.txt")))
			{
				File.Delete(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\fridge.txt"));
			}
			File.Copy(System.IO.Path.Combine(directory, freezer), System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\fridge.txt"));
			Process.Start(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\fridge.txt"));
		}

		private void PrintOrder_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\order.txt")))
			{
				File.Delete(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\order.txt"));
			}
			File.Copy(System.IO.Path.Combine(directory, order), System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\order.txt"));
			Process.Start(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\order.txt"));
		}
		private void SaveFiles()
		{
			using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(directory, freezer), false))
			{
				for (int i = 0; i < f.Count; i++)
				{
					sw.WriteLine(f[i]);
				}
			}
			using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(directory, order), false))
			{
				for (int i = 0; i < o.Count; i++)
				{
					sw.WriteLine(o[i]);
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
		private bool Find(string s, List<string> collection)
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
				MessageBoxResult mr = MessageBox.Show("У вас есть пункты, частично или полностью содержащие " + s + ":\n" + res + "Добавить в список?", "Результаты поиска", MessageBoxButton.YesNo);
				if (mr == MessageBoxResult.Yes)
				{
					return true;
				}
				else return false;
			}
			return true;
		}

		private void EnglishVersionButton_Click(object sender, RoutedEventArgs e)
		{
			locale = "en-US";
			ChangeLanguage(locale);
		}

		private void RussianVersionButton_Click(object sender, RoutedEventArgs e)
		{
			locale = "ru-RU";
			ChangeLanguage(locale);
		}
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
				StuffToBuyText.Text = "Будем есть (добавить - наберите название,\nудалить - введите номер)";
				RussianVersionButton.IsEnabled = false;
				RussianVersionButton.Visibility = Visibility.Hidden;
				EnglishVersionButton.IsEnabled = true;
				EnglishVersionButton.Visibility = Visibility.Visible;
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
				StuffToBuyText.Text = "Stuff to buy\nadd - type the name,\n remove - type the number";
				EnglishVersionButton.IsEnabled = false;
				EnglishVersionButton.Visibility = Visibility.Hidden;
				RussianVersionButton.IsEnabled = true;
				RussianVersionButton.Visibility = Visibility.Visible;
			}
            else if (locale == "")
            {
				locale = "ru-RU";
				ChangeLanguage(locale);
			}
			else
			{
				MessageBox.Show("Internal error!\nPlease reinstall the application.\nYou are welcome to find fresh release here:\nhttps://pegor.keenetic.pro/Setup.msi");
				Environment.Exit(0);
			}
		}
	}
}
