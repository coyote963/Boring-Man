using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Media;
using System.Globalization;
//todo::
//format the string output better
//write methods in parse to loaddata
//write methods to search loaded data
//write methods to update loaded data
//write methods to save loaded data
public static class BMConstants
{
	//text for when someone wants to ping you
	public const string HostPing = "!ping";
	//text for USC winning
	public const string USC_WINNER_TEXT = "Daberoni WINS";
	//text for a draw
	public const string TIE_TEXT = "DRAW";
	//winner text for man
	public const string MAN_WINNER_TEXT = "Intense WINS";
	//the password to your rcon.
	public const string RConPW = "smooby";
	//the file where end of match data is held.
	public const string DataFileName = "data.txt";
	//the file where the chat input output is held. Each input and output pair is separated by a new line. Use a '|' to split up input and output
	public const string ChatListName = "chat_list.txt";
	//the file where the player data is held. 
	public const string PlayerListName = "players.txt";
	//the maximum change per kill. Higher makes ratings more volatile, lower means climbing rating is harder
	public const int MAX_CHANGE = 32;
}
public class BMConsoleTester
{
	public static void Main(string[] args)
	{
		List<Player> players = ELOScore.ReadFromFile(BMConstants.PlayerListName);

		TcpClient myclient = MyStreamHelper.Connect();

		MyStreamHelper.Login(BMConstants.RConPW,myclient);

		List<FinalMatchData> list = new List<FinalMatchData>();
		StreamWriter sw = new StreamWriter(BMConstants.DataFileName,true);

		sw.WriteLine("Initialized");
		List<Conversation> conversations = Responder.CreateConversationList(BMConstants.ChatListName);
		
		
		while(!Console.KeyAvailable)
		{

			Thread.Sleep(1000);
			Parser.Parse(MyStreamHelper.ReadFromStream(myclient),list, myclient,conversations,players);
			Parser.SaveToFile(sw,list);
			list.Clear();
			ELOScore.WriteToFile(players,BMConstants.PlayerListName);
		}
		Console.WriteLine("You pressed a key: Quitting");
		sw.Flush()
;		sw.Close();
		myclient.Close();
		// List<Conversation> conversations = Responder.CreateConversationList("chat_list.txt");
		// Console.WriteLine(Responder.Respond(conversations,"!data pistol\n"));
	}
}
public class Parser
{
	public static void Parse(string[] datarray, List<FinalMatchData> list,TcpClient myclient,List<Conversation> myconversationlist,List<Player> playerlist) 
	{
		for (int i = 0; i< datarray.Length-1; )
		{
			switch (datarray[i])
			{
				case "1":
					Console.WriteLine("parsed a one");
					String[] fullcomment = Parser.ChatSplit(datarray[i+1]);

					string s ="";
					if ((fullcomment!=null)&&(fullcomment[1].StartsWith("!")))
					{
						s = Responder.Respond(myconversationlist,playerlist,fullcomment[0],fullcomment[1]);
						
					}

					if (s !="")
					{
						
						MyStreamHelper.WriteToChat(s+"\n",myclient);
					}
					i = i + 3;
				break;
				case "2":
					Console.WriteLine("parsed a 2");
					list.Add(new MatchOverData(datarray[i+1]));	
					
					MyStreamHelper.WriteToChat("/info"+"\n",myclient);
					MyStreamHelper.WriteToChat("/scoreboard"+"\n",myclient);
					i = i + 3;
					break;
				case "3":
					Console.WriteLine("parsed a 3");
					list.Add(new ScoreboardData(datarray[i+2],datarray[i+3],datarray[i+5],datarray[i+6]));
					i = i + 7;
					break;
				case "4":
					Console.WriteLine("parsed a 4");
					list.Add(new InfoData(datarray[i+5]));
					i = i + 8;
					break;
				case "5":
					Console.WriteLine("parsed a 5");
					i = i + 3;
					break;
				case "6":
					Console.WriteLine("parsed a 6");
					ELOScore.MatchupOccured(datarray[i+1],datarray[i+2],playerlist);
					i = i + 4;
					break;
				default:
					throw new System.ArgumentException("Cannot Parse: Not one of the enumerated info types: "+datarray[i]);
			}
		}
		
	}
	public static string[] ChatSplit(String chat){
		String[] result =  Regex.Split(chat,": ");
		if (result.Length == 2){
			return result;
		}else return null;
	}
	public static bool SaveToFile(StreamWriter sw,List<FinalMatchData> templist)
	{
		try
		{
			foreach (FinalMatchData matchdata in templist)
			{
				sw.WriteLine(matchdata.ConvertToString());
				sw.Flush();
			}
			return true;
		}
		catch (Exception ex)
		{
            Console.Write(ex.Message);
            Console.Write(ex.StackTrace);
            return false;
        }
	}
}
public class MyStreamHelper
{
	static char delimit = '\n';
	public static TcpClient Connect()
	{
		TcpClient client = new TcpClient();
		try
		{
			
			//create a tcpclient
			//not, for this client to work you need to have a tcpserver
			//StreamHelpered to the same address as specified by the server, port combination
			Int32 port = 7780;
			string ipaddress = "76.127.76.232";
			//Get the ipaddress of the host as a byte array
			System.Net.IPAddress ip = System.Net.IPAddress.Parse(ipaddress);

			//Initialize and StreamHelper the client
			
			client.Connect(ip,port);
			
		}
		catch (ArgumentNullException e) 
		{
			Console.WriteLine("ArgumentNullException: {0}", e);
			return null;
		} 
		catch (SocketException e) 
		{
			Console.WriteLine("SocketException: {0}", e);
			return null;
		}

		Console.WriteLine("\n Starting ...");
		return client;

	}
	public static void WriteToChat(String message,TcpClient client)
	{
 		//translate the passed message into ASCII and store it as a byte array
		NetworkStream stream = client.GetStream();
		StreamWriter sw = new StreamWriter(stream);
		sw.Write(message);
		sw.Flush();
	}
	public static void Login(String password,TcpClient client)
	{
		//logs in to the remote console
		NetworkStream stream = client.GetStream();
		StreamWriter sw = new StreamWriter(stream);
		sw.Write("/rcon "+password+delimit);
		sw.Flush();
	}
	public static void Info(TcpClient client)
	{
		NetworkStream stream = client.GetStream();
		StreamWriter sw = new StreamWriter(stream);
		sw.Write("/info"+delimit);
		sw.Flush();
	}
	public static void Scoreboard(TcpClient client)
	{
		NetworkStream stream = client.GetStream();
		StreamWriter sw = new StreamWriter(stream);
		sw.Write("/scoreboard"+delimit);
		sw.Flush();
	}
	public static string[] ReadFromStream(TcpClient client)
	{
		NetworkStream stream = client.GetStream();
		try
		{
			// TextReader reader= new StreamReader(stream);		
			// Console.Write(reader.ReadToEnd());
			if (stream.CanRead) {
				byte[] myReadBuffer = new Byte[4096];
				StringBuilder message = new StringBuilder();
				int numberOfBytesRead = 0;
				do 
				{
					
					numberOfBytesRead = stream.Read(myReadBuffer,0,myReadBuffer.Length);
					message.AppendFormat("{0}",Encoding.ASCII.GetString(myReadBuffer,0,numberOfBytesRead));
				}
				while(stream.DataAvailable);
				Console.WriteLine(message);
				return message.ToString().Split('\n');
			}
			else{

				Console.WriteLine("Sorry. You cannot read from this NetworkStream");
				return null;
			}
		}
		catch(Exception ex)
		{
			Console.WriteLine(ex.Message+ex.StackTrace);
			return null;
		}
	}
}

public class MatchOverData : FinalMatchData
{

	string whoWin;
	string currentTime;
	public MatchOverData(string matchwinner)
	{
		if (matchwinner == BMConstants.USC_WINNER_TEXT)
		{
			whoWin = "usc";
		}
		else
		{
			if (matchwinner == BMConstants.TIE_TEXT)
			{
				whoWin = "tie";
			}
			else 
			{
				if (matchwinner == BMConstants.MAN_WINNER_TEXT) 
				{
					whoWin = "man";
				}
				else 
				{

					throw new System.ArgumentException("Cannot Parse: Not one of the Winner Texts: "+matchwinner);
				}
			}
		}
		currentTime = DateTime.Now.ToString();
	}
	public string ConvertToString()
	{
		return "\nMatch: "+ whoWin+"\t"+currentTime;
	}
}

public class ScoreboardData : FinalMatchData
{
	string team;
	string name;
	string kills;
	string deaths;
	public ScoreboardData(string team, string name, string kills, string deaths)
	{
		switch(Int32.Parse(team)){
			case 1:
				this.team = "man";
				break;
			case 2:
				this.team = "usc";
				break;
			case 3:
				this.team = "spc";
				break;
			default:
				throw new System.ArgumentException("Cannot Parse: Not one of the enumerated teams");
		}
		this.name = name;
		this.kills = kills;
		this.deaths = deaths;
	}
	public string ConvertToString()
	{
		return team + "\t" + name + "\t\t\t" + kills + "\t"+ deaths + "\t";
	}

}

public class InfoData : FinalMatchData
{
	string mapname;
	public InfoData(string mapname)
	{
		this.mapname = mapname;
	}
	public string ConvertToString()
	{
		return mapname;
	}
}

public interface FinalMatchData
{
	string ConvertToString();
}

public class Responder
{
	public static List<Conversation> CreateConversationList(String filename)
	{
		StreamReader sr = new StreamReader(filename);
		string line = string.Empty;
		List<Conversation> conversationlist= new List<Conversation>();
		while ((line = sr.ReadLine()) != null)
		{
			
			string[] stringarray =line.Split('|');

			conversationlist.Add(new Conversation(stringarray[0],stringarray[1]));
		}
		return conversationlist;
	}
	public static string Respond(List<Conversation> conversationlist,List<Player> playerlist, string username, string usercommand)
	{
		
		if (usercommand == "!ranking")
		{
			Console.WriteLine("{0} requested Rating",username);
			Player p = ELOScore.SearchPlayer(playerlist,username);
			return p.ConvertToString();
		}
		if (usercommand == BMConstants.HostPing)
		{
			SystemSounds.Beep.Play();	
			return null;
		}
		if (usercommand == BMConstants.RequestTopTen)
		{
			playerlist.Sort();
			string result;
			int iter=1;
			foreach (Player p in playerlist)
			{
				result += "rank: "+iter+p.GetName()+"-"p.GetRanking();
				if (++iter == 5) break;  
			}
		}
		foreach (Conversation x in conversationlist)
		{
			if (x.GetCommand()==usercommand)
			{
				return x.GetResponse();
			}
		}return null;
	}
}

public class Conversation
{
	string command;
	string response;
	public Conversation(string command, string response)
	{
		this.command = command;
		this.response= response;
	}
	public string GetCommand()
	{
		return command;
	}
	public string GetResponse()
	{
		return response;
	}
}
public class Player : IComparable<Player>
{
	double ELO;
	string username;
	string ranking = "Canadian Cannibal";
	public Player(string username, double ELO)
	{
		this.username = username;
		this.ELO = ELO;
	}
	public string GetRanking()
	{
		return ELO + ranking;
	}
	public double GetELO()
	{
		return ELO;
	}
	public string GetName()
	{
		return username;
	}
	public string ConvertToString()
	{
		return username + "-" + ranking + "-"+ELO;
	}
	public void SetELO(double ELO)
	{
		Console.WriteLine(username + " new ELO " + ELO);
		this.ELO = ELO;
		if (ELO > 1000)
		{
			ranking = "Cannibal";
		}
		if (ELO > 1050)
		{
			ranking = "Canadian Cannibal";
		}
		if (ELO > 1100)
		{
			ranking = "Hopper";
		}
		if (ELO > 1150)
		{
			ranking = "Insane Hopper";
		}
		if (ELO > 1200)
		{
			ranking = "Blue Soldier";
		}
		if (ELO > 1250)
		{
			ranking = "Expert Blue Soldier";
		}
		if (ELO > 1300)
		{
			ranking = "Purple";
		}
		if (ELO > 1350)
		{
			ranking = "Ancient Purple";
		}
		if (ELO > 1400)
		{
			ranking = "Ninja";
		}
		if (ELO > 1450)
		{
			ranking = "Skilled Ninja";
		}
		if (ELO > 1500)
		{
			ranking = "Bomb Dude";
		}
		if (ELO > 1550)
		{
			ranking = "Disciple";
		}
		if (ELO > 1600)
		{
			ranking = "Manling";
		}
		if (ELO > 1650)
		{
			ranking = "Godless Manling";
		}
		if (ELO > 1700)
		{
			ranking ="Inhabitant of Nightmares - The Man";
		}
		if (ELO > 1750)
		{
			ranking = "Destroyer of Worlds - The Man";
		}
		if (ELO > 1800)
		{
			ranking = "Hero of Somewhere Land - BORING MAN";
		}
		if (ELO > 1850)
		{
			ranking = "World's Greatest - BORING MAN";
		}
	}
	public int CompareTo(Player other)
	{
		if (this.GetELO() == other.GetELO())
		{
			return this.GetName().CompareTo(other.GetName());

		}
		return other.GetELO().CompareTo(this.GetELO());
	}
}
public class ELOScore
{
	public static void MatchupOccured(String winner, String loser, List<Player> list)
	{

		Player winPlayer = SearchPlayer(list,winner);
		Player losePlayer = SearchPlayer(list,loser);
		//get the ELO, little r
		double winPlayerELO = winPlayer.GetELO();
		double losePlayerELO = losePlayer.GetELO();
		//get the transformed ELO, big R
		double transformedWinPlayerELO =  Math.Pow(10,winPlayerELO/400);
		double transformedLosePlayerELO =  Math.Pow(10,losePlayerELO/400);
		//get the expected score E
		double expectedWinPlayerELO = transformedWinPlayerELO/(transformedWinPlayerELO+transformedLosePlayerELO);
		double expectedLosePlayerELO = transformedLosePlayerELO/(transformedWinPlayerELO+transformedLosePlayerELO);

		winPlayer.SetELO(winPlayerELO + BMConstants.MAX_CHANGE*(1-expectedWinPlayerELO));
		losePlayer.SetELO(losePlayerELO-BMConstants.MAX_CHANGE*expectedLosePlayerELO);
	}
	public static void WriteToFile(List<Player> list, string filename)
	{
		list.Sort();
		StreamWriter sw = new StreamWriter(filename);

		foreach (Player p in list)
		{
			
			sw.WriteLine(p.ConvertToString());
		}
		sw.Flush();
		sw.Close();
	}
	public static List<Player> ReadFromFile(string filename)
	{
		StreamReader sr = new StreamReader(filename);
		string line;
		List<Player> list = new List<Player>();
		while ((line = sr.ReadLine()) != null)
		{
			String[] linearray = line.Split('-');
			if (linearray.Length == 3)
			{
				Console.WriteLine(linearray[0]);

				list.Add(new Player(linearray[0], double.Parse(linearray[2], CultureInfo.InvariantCulture)));
			}
		}
		sr.Close();
		return list;

	}
	public static Player SearchPlayer(List<Player> list, string name)
	{
		foreach (Player p in list)
		{
			if (p.GetName() == name)
			{
				return p;
			}
		}

		Player newplayer = new Player(name, 1000);
		list.Add(newplayer);
		return newplayer;
	}

}
