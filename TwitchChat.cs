using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.IO;
using UnityEngine.UI;

public class TwitchChat : MonoBehaviour
{

    public string username;
    public string password; // Get password from https://twitchapps.com/tmi
    public string channelName;
    public Text chatBox;

    private TcpClient twitchClient;
    private StreamReader reader;
    private StreamWriter writer;

    // Global temp variables
    string mostRecentChatName = "";

    // Use this for initialization
    void Start()
    {
        Connect();
    }

    // Update is called once per frame
    void Update()
    {
        if (!twitchClient.Connected)
        {
            Connect();
        }

        ReadChat();
    }

    // Connects to the twitch client
    private void Connect()
    {
        twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
        reader = new StreamReader(twitchClient.GetStream());
        writer = new StreamWriter(twitchClient.GetStream());

        writer.WriteLine("PASS " + password);
        writer.WriteLine("NICK " + username);
        writer.WriteLine("USER " + username + " 8 * :" + username);
        writer.WriteLine("JOIN #" + channelName);
        writer.Flush();
    }

    // Read the chat
    private void ReadChat()
    {
        if (twitchClient.Available > 0)
        {
            var message = reader.ReadLine(); // Read the current message

            // Keep connection alive by replying to regular pings
            if (message.Contains("PING")) { writer.WriteLine("PONG :tmi.twitch.tv"); writer.Flush(); }

            if (message.Contains("PRIVMSG"))
            {
                // Get the users name by splitting it from the string
                int splitPoint = message.IndexOf("!", 1); // was type Var
                string chatName = message.Substring(0, splitPoint); // was type Var
                chatName = chatName.Substring(1);
                mostRecentChatName = chatName;

                // Get the users message by splitting it from the string
                splitPoint = message.IndexOf(":", 1);
                message = message.Substring(splitPoint + 1);
                // Add new line to the ChatBox UI object!
                if (chatBox != null)
                {
                    chatBox.text = string.Format("<color=blue>{0}:</color> <color=green>{1}</color>", chatName, message)
                        + "\n" + chatBox.text;
                }
            }

            print(message);

            // if first character is '!' then pass along to Game Manager to handle command
            if (message[0] == '!' && message.Length < 10)
            {
                GameManager.Instance.HandleCommand(mostRecentChatName, message);

                switch (message)
                {
                    case "!help":
                    case "!instructions":
                    case "!howtoplay":
                        writer = new StreamWriter(twitchClient.GetStream());
                        string messageToSend = "Instructions: To join game type !play ...Type !task to work on your task. Type !heal to restore your health. Type !ability to perform special ability once it is earned. Don't die and GOOD LUCK!";
                        writer.WriteLine(string.Format(":{0}!{0}@{0}.tmi.twitch.tv PRIVMSG #{1} :{2}", username, channelName, messageToSend));
                        writer.Flush();
                        break;

                    default:
                        break;
                }
                // message.Substring(0, message.IndexOf(" "))
            }
        }
    }
}
