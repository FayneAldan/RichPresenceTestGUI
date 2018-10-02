using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Threading;

namespace RichPresenceTestGUI {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private DiscordRpc.EventHandlers handlers;
		private DispatcherTimer timer;

		public MainWindow() {
			InitializeComponent();
			timer = new DispatcherTimer();
			timer.Tick += DoCallbacks;
			timer.Interval = new TimeSpan(0, 0, 1);
			timer.Start();
		}

		private void DoShutdown(object sender, System.ComponentModel.CancelEventArgs e) {
			timer.Stop();
			DiscordRpc.Shutdown();
		}

		private void DoCallbacks(object sender, EventArgs e) {
			DiscordRpc.RunCallbacks();
		}

		int LogCount = 0;

		private void Log(string[] msgs) {
			Log(String.Join("\n", msgs));
		}
		private void Log(string msg) {
			msg = msg.TrimEnd(new[] { '\n' });
			Console.WriteLine("\n" + msg);
			LogPanel.Children.Insert(0, new Separator());
			LogPanel.Children.Insert(0, new TextBlock {
				Text = msg,
				TextWrapping = TextWrapping.Wrap
			});
			if (!LogTab.IsSelected)
				LogCount++;
			LogTab.Header = "[" + LogCount + "] Log";
		}

		private void DoInitialize(object sender, RoutedEventArgs e) {
			string clientId = ClientID.Text;
			bool autoRegister = AutoRegister.IsChecked ?? false;
			string steamId = SteamID.Text;
			if (steamId.Length == 0)
				steamId = null;
			handlers = new DiscordRpc.EventHandlers {
				readyCallback = ReadyCallback,
				disconnectedCallback = DisconnectedCallback,
				errorCallback = ErrorCallback,
				joinCallback = JoinCallback,
				spectateCallback = SpectateCallback,
				requestCallback = RequestCallback
			};
			Log(new string[] {
				"[Method] Initialize",
				"Client ID: " + clientId,
				"Auto Register: " + autoRegister,
				"Steam ID: " + (steamId ?? "null")
			});
			DiscordRpc.Initialize(clientId, ref handlers, autoRegister, steamId);
			DiscordRpc.UpdatePresence(new DiscordRpc.RichPresence() {
				state = "Doing Some Testing",
				details = "Braking C#",
				partyId = "123456789",
				//partySize = 1,
				//partyMax = 999,
				spectateSecret = "123ABC",
				joinSecret = "ABC123"
			});
		}

		private void PresenceClear(object sender, RoutedEventArgs e) {
			Log("[Method] ClearPresence");
			DiscordRpc.ClearPresence();
		}

		private void ReadyCallback(ref DiscordRpc.DiscordUser user) {
			Log(new string[] {
				"[Callback] Ready",
				"User ID: " + user.userId,
				"Username: " + user.username,
				"Discriminator: " + user.discriminator,
				"Avatar: " + user.avatar
			});
		}

		private void ReadyCallback() {
			Log(new string[] {
				"[Callback] Ready",
				"User information ignored to avoid crash."
			});
		}

		private void DisconnectedCallback(int errorCode, string message) {
			Log(new string[] {
				"[Callback] Disconnected",
				"Error Code: " + errorCode,
				"Message: " + message
			});
		}

		private void ErrorCallback(int errorCode, string message) {
			Log(new string[] {
				"[Callback] Error",
				"Error Code: " + errorCode,
				"Message: " + message
			});
		}

		private void JoinCallback(string secret) {
			Log(new string[] {
				"[Callback] Join",
				"Secret: " + secret
			});
		}

		private void SpectateCallback(string secret) {
			Log(new string[] {
				"[Callback] Spectate",
				"Secret: " + secret
			});
		}

		private void RequestCallback(ref DiscordRpc.DiscordUser user) {
			Log(new string[] {
				"[Callback] Request",
				"User ID: " + user.userId,
				"Username: " + user.username,
				"Discriminator: " + user.discriminator,
				"Avatar: " + user.avatar
			});
			if (!RespondTab.IsSelected) {
				RespondTab.Header = "[!] Respond";
				RespondID.Text = user.userId;
				RespondName.Content = user.username + "#" + user.discriminator + "\n(" + user.userId + ")";
			} else
				LogTab.Header = "[!] Log";
		}

		private void RemoveRespondName(object sender, TextChangedEventArgs e) {
			RespondName.Content = "";
		}

		private void Respond(DiscordRpc.Reply reply) {
			string userId = RespondID.Text;
			Log(new string[] {
				"[Method] Respond",
				"User ID: " + userId,
				"Reply: " + reply.ToString()
			});
			DiscordRpc.Respond(userId, reply);
		}

		private void RespondNo(object sender, RoutedEventArgs e) {
			Respond(DiscordRpc.Reply.No);
		}

		private void RespondYes(object sender, RoutedEventArgs e) {
			Respond(DiscordRpc.Reply.Yes);
		}

		private void RespondIgnore(object sender, RoutedEventArgs e) {
			Respond(DiscordRpc.Reply.Ignore);
		}

		private void ChangeTab(object sender, SelectionChangedEventArgs e) {
			TabItem tab = (TabItem) ((TabControl) sender).SelectedValue;
			if (tab.Equals(LogTab))
				LogTab.Header = "[0] Log";
			else if (tab.Equals(RespondTab))
				RespondTab.Header = "Respond";
		}
	}
}
