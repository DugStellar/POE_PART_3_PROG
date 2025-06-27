using System;
using System.IO;
using NAudio.Wave;
using System.Collections.Generic;
using System.Linq;
using System.Globalization; // Needed for parsing dates

public class InfoGuardCombined
{
    public string UserName { get; set; }
    private Dictionary<string, Dictionary<Sentiment, List<string>>> sentimentAwareResponses =
        new Dictionary<string, Dictionary<Sentiment, List<string>>>()
        {
            { "password", new Dictionary<Sentiment, List<string>>()
                {
                    { Sentiment.Neutral, new List<string> {
                        "Make sure to use strong, unique passwords for each account. Avoid using personal details in your passwords.",
                        "A strong password typically includes a mix of uppercase and lowercase letters, numbers, and symbols.",
                        "Consider using a password manager to securely store and generate complex passwords."
                    }},
                    { Sentiment.Worried, new List<string> {
                        "Feeling worried about password security is understandable. Let's focus on creating strong ones.",
                        "It's wise to be concerned about password safety. I can give you some tips to ease your worries."
                    }}
                }
            },
            { "scam", new Dictionary<Sentiment, List<string>>()
                {
                    { Sentiment.Neutral, new List<string> {
                        "Be wary of online scams that may try to trick you into giving away personal information or money.",
                        "If something sounds too good to be true, it probably is a scam.",
                        "Never share sensitive information with unverified sources."
                    }},
                    { Sentiment.Worried, new List<string> {
                        "It's understandable to feel worried about online scams. They can be very convincing. Let's discuss some ways to identify and avoid them.",
                        "Being concerned about scams is a good sign! I can share some tips to help you feel more secure."
                    }}
                }
            },
            { "privacy", new Dictionary<Sentiment, List<string>>()
                {
                    { Sentiment.Neutral, new List<string> {
                        "Review your privacy settings on online accounts to control who sees your information.",
                        "Be mindful of the information you share online.",
                        "Consider using privacy-focused tools and services."
                    }},
                    { Sentiment.Worried, new List<string> {
                        "It's natural to be worried about online privacy. There are steps you can take to protect your personal information.",
                        "Concerns about privacy are valid. Let's explore some ways to enhance your online privacy."
                    }}
                }
            },
            { "phishing", new Dictionary<Sentiment, List<string>>()
                {
                    { Sentiment.Neutral, new List<string> {
                        "Phishing attempts often come in the form of emails or messages that look legitimate but are designed to steal your information.",
                        "Never click on suspicious links or open attachments from unknown senders.",
                        "Verify the sender's authenticity through official channels if you are unsure."
                    }},
                    { Sentiment.Worried, new List<string> {
                        "Phishing can be a real concern. I can provide some guidance on how to spot and avoid phishing attempts.",
                        "Feeling uneasy about phishing is understandable. Let me share some crucial tips."
                    }}
                }
            },
            { "malware", new Dictionary<Sentiment, List<string>>()
                {
                    { Sentiment.Neutral, new List<string> {
                        "Malware is malicious software that can harm your device or steal your data. Install reputable antivirus software and keep it updated.",
                        "Be cautious when downloading files or installing software from the internet.",
                        "Avoid clicking on suspicious ads or pop-ups."
                    }},
                    { Sentiment.Worried, new List<string> {
                        "Worrying about malware is reasonable. Protecting your devices is important. Let's talk about how to do that.",
                        "It's wise to be concerned about malware. I can give you some advice on prevention."
                    }}
                }
            },
            { "online safety", new Dictionary<Sentiment, List<string>>()
                {
                    { Sentiment.Neutral, new List<string> {
                        "Practice safe Browse habits by avoiding suspicious websites and using secure connections (HTTPS).",
                        "Keep your software and operating system updated to patch security vulnerabilities.",
                        "Be aware of social engineering tactics that criminals use to manipulate you."
                    }},
                    { Sentiment.Worried, new List<string> {
                        "Feeling worried about overall online safety is common. Let's break it down into manageable steps.",
                        "It's good that you're thinking about online safety. I can offer some fundamental guidelines."
                    }}
                }
            },
            { "personal information", new Dictionary<Sentiment, List<string>>()
                {
                    { Sentiment.Neutral, new List<string> {
                        "Be cautious about sharing personal information online. Only provide it to trusted sources when necessary.",
                        "Securely store any sensitive personal data and avoid keeping it unnecessarily.",
                        "Be aware of data breaches and take steps to protect your accounts if your information may have been compromised."
                    }},
                    { Sentiment.Worried, new List<string> {
                        "Concerns about protecting personal information online are valid. Let's discuss how to minimize risks.",
                        "It's important to be careful with your personal information. I can share some best practices."
                    }}
                }
            },
            { "how are you", new Dictionary<Sentiment, List<string>>()
                {
                    { Sentiment.Neutral, new List<string> { "I'm doing well, thank you! Ready to help you with cybersecurity." } }
                }
            },
            { "what's your purpose", new Dictionary<Sentiment, List<string>>()
                {
                    { Sentiment.Neutral, new List<string> { "My purpose is to educate you about cybersecurity best practices." } }
                }
            },
            { "what can i ask you about", new Dictionary<Sentiment, List<string>>()
                {
                    { Sentiment.Neutral, new List<string> { "You can ask me about:\n- Password safety\n- Phishing\n- Safe Browse\n- And other cybersecurity topics!" } }
                }
            }
        };

    private List<string> phishingTips = new List<string>()
    {
        "Be cautious of emails asking for personal information.",
        "Scammers often disguise themselves as trusted organizations.",
        "Check the sender's email address carefully for any inconsistencies.",
        "Never click on links in suspicious emails.",
        "If in doubt, contact the organization directly through a verified channel."
    };

    private Dictionary<string, string> memory = new Dictionary<string, string>();
    private Dictionary<string, Sentiment> sentimentHistory = new Dictionary<string, Sentiment>();
    private Dictionary<string, string> lastResponse = new Dictionary<string, string>(); // To avoid immediate repetition

    private enum Sentiment { Worried, Curious, Frustrated, Neutral }

    // --- Task 1: Task Assistant with Reminders ---
    public class CybersecurityTask
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }

        public override string ToString()
        {
            string reminderInfo = ReminderDate.HasValue ? $" (Reminder: {ReminderDate.Value.ToShortDateString()})" : "";
            string status = IsCompleted ? "[COMPLETED] " : "";
            return $"{status}{Title}: {Description}{reminderInfo}";
        }
    }
    private List<CybersecurityTask> cybersecurityTasks = new List<CybersecurityTask>();

    // --- Task 2: Cybersecurity Mini-Game (Quiz) ---
    public class QuizQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public int CorrectOptionIndex { get; set; } // 0-indexed
        public string Explanation { get; set; }

        public QuizQuestion(string question, List<string> options, int correctIndex, string explanation)
        {
            Question = question;
            Options = options;
            CorrectOptionIndex = correctIndex;
            Explanation = explanation;
        }
    }
    private List<QuizQuestion> quizQuestions;
    private int currentQuizScore = 0;
    private int currentQuizQuestionIndex = 0;
    private bool inQuizMode = false;


    // --- Task 4: Activity Log Feature ---
    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"{Timestamp.ToString("yyyy-MM-dd HH:mm:ss")}: {Description}";
        }
    }
    private List<ActivityLogEntry> activityLog = new List<ActivityLogEntry>();

    public InfoGuardCombined()
    {
        InitializeQuizQuestions();
    }

    private void LogActivity(string description)
    {
        activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, Description = description });
    }

    public void Run()
    {
        PlayVoiceGreeting();
        DisplayAsciiArt();
        GreetUser();
        HandleUserInteraction();
    }

    private void PlayVoiceGreeting()
    {
        try
        {
            string soundFilePath = "greeting.wav";

            if (File.Exists(soundFilePath))
            {
                try
                {
                    using (var audioFile = new AudioFileReader(soundFilePath))
                    using (var outputDevice = new WaveOutEvent())
                    {
                        outputDevice.Init(audioFile);
                        outputDevice.Play();
                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
                catch (Exception audioEx)
                {
                    Console.WriteLine($"Error during audio playback: {audioEx.Message}");
                }

            }
            else
            {
                Console.WriteLine("Warning: Voice greeting file not found. Place 'greeting.wav' in the application's directory.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accessing audio file: {ex.Message}");
        }
    }

    private void DisplayAsciiArt()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
    /\_/\
   ( o.o )
    > ^ <  InfoGuard
   /  _  \
  | (_) |  Meow! Let's stay safe online!
  \_____/
");
        Console.ResetColor();
    }

    private void GreetUser()
    {
        Console.Write("Please enter your name: ");
        UserName = Console.ReadLine();

        if (string.IsNullOrEmpty(UserName))
        {
            UserName = "User";
        }

        Console.WriteLine($"\nHello, {UserName}! Welcome to InfoGuard.");
        LogActivity($"User greeted: {UserName}");
    }

    private void HandleUserInteraction()
    {
        string previousInput = null;
        while (true)
        {
            // Check for reminders
            CheckAndDisplayReminders();

            Console.Write($"\nAsk me a cybersecurity question, {UserName} (or type 'exit' to quit): ");
            string userInput = Console.ReadLine();

            if (userInput.ToLower() == "exit")
            {
                Console.WriteLine("Goodbye! Stay safe online.");
                LogActivity("Chatbot exited.");
                break;
            }

            if (inQuizMode)
            {
                HandleQuizAnswer(userInput);
            }
            else
            {
                string response = GetResponse(userInput, previousInput);
                Console.WriteLine(response);
                previousInput = userInput;
            }
        }
    }

    private void CheckAndDisplayReminders()
    {
        var upcomingReminders = cybersecurityTasks
            .Where(t => t.ReminderDate.HasValue && t.ReminderDate.Value.Date == DateTime.Today.Date && !t.IsCompleted)
            .ToList();

        if (upcomingReminders.Any())
        {
            Console.WriteLine("\n--- Reminder! ---");
            foreach (var task in upcomingReminders)
            {
                Console.WriteLine($"Reminder for today: {task.Title} - {task.Description}");
                LogActivity($"Displayed reminder for task: {task.Title}");
            }
            Console.WriteLine("-----------------\n");
        }
    }


    // --- Task 3: Natural Language Processing (NLP) Simulation & Task 1 Integration ---
    private string GetResponse(string userInput, string previousInput)
    {
        string lowerInput = userInput.ToLower();
        Sentiment currentSentiment = DetectSentiment(lowerInput);
        if (!string.IsNullOrEmpty(UserName))
        {
            sentimentHistory[UserName] = currentSentiment;
        }

        // --- Task 1: Task Assistant Commands ---
        // Add Task
        if (lowerInput.Contains("add task") || lowerInput.Contains("create task") || lowerInput.Contains("set up task"))
        {
            return AddTask(userInput);
        }
        // View Tasks
        else if (lowerInput.Contains("view tasks") || lowerInput.Contains("show tasks") || lowerInput.Contains("list tasks"))
        {
            return ViewTasks();
        }
        // Mark Task Complete
        else if (lowerInput.Contains("complete task") || lowerInput.Contains("mark task complete"))
        {
            return MarkTaskComplete(userInput);
        }
        // Delete Task
        else if (lowerInput.Contains("delete task") || lowerInput.Contains("remove task"))
        {
            return DeleteTask(userInput);
        }
        // Set Reminder (can be part of add task or standalone)
        else if (lowerInput.Contains("set reminder for") || lowerInput.Contains("remind me about"))
        {
            // This assumes a task might already exist or the user is trying to set a reminder for a conceptual task.
            // A more robust system would link reminders to existing tasks or create new ones.
            string reminderPrompt = "Please tell me the task title and when you want to be reminded (e.g., 'Remind me about password change tomorrow' or 'Set reminder for 'Review privacy' on 2025-07-15'):";
            LogActivity($"Requested reminder for: {userInput}");
            return reminderPrompt; // We'll handle the actual parsing in a follow-up or dedicated "AddTask"
        }
        // Specific reminder setting from previous input (very basic example)
        else if (previousInput != null && previousInput.Contains("tell me the task title") && lowerInput.Contains("remind me in"))
        {
            return SetReminderFromFollowUp(previousInput, lowerInput);
        }

        // --- Task 2: Quiz Commands ---
        else if (lowerInput.Contains("start quiz") || lowerInput.Contains("play quiz") || lowerInput.Contains("cybersecurity quiz"))
        {
            return StartQuiz();
        }

        // --- Task 4: Activity Log Commands ---
        else if (lowerInput.Contains("show activity log") || lowerInput.Contains("what have you done for me"))
        {
            return ShowActivityLog();
        }


        // Memory Recall
        else if (lowerInput.StartsWith("remember my name is"))
        {
            string remainingText = lowerInput.Substring("remember my name is".Length).Trim();
            if (!string.IsNullOrEmpty(remainingText))
            {
                UserName = remainingText;
                memory["name"] = remainingText;
                LogActivity($"Remembered user name: {UserName}");
                return $"Okay, I'll remember your name is {remainingText}.";
            }
            else
            {
                return "Please provide the name you want me to remember.";
            }
        }
        else if (lowerInput.StartsWith("remember my favorite cybersecurity topic is"))
        {
            string remainingText = lowerInput.Substring("remember my favorite cybersecurity topic is".Length).Trim();
            if (!string.IsNullOrEmpty(remainingText))
            {
                string topic = remainingText;
                memory["favorite_topic"] = topic;
                LogActivity($"Remembered favorite topic: {topic}");
                return $"Got it! I'll remember that your favorite cybersecurity topic is {topic}.";
            }
            else
            {
                return "Please provide your favorite cybersecurity topic.";
            }
        }
        else if (lowerInput.Contains("what do you remember about me"))
        {
            if (memory.Count > 0)
            {
                string memoryRecall = "I remember ";
                if (memory.ContainsKey("name"))
                {
                    memoryRecall += $"your name is {memory["name"]}";
                    if (memory.ContainsKey("favorite_topic"))
                    {
                        memoryRecall += $" and your favorite cybersecurity topic is {memory["favorite_topic"]}.";
                    }
                    else
                    {
                        memoryRecall += ".";
                    }
                }
                else if (memory.ContainsKey("favorite_topic"))
                {
                    memoryRecall += $"your favorite cybersecurity topic is {memory["favorite_topic"]}.";
                }
                LogActivity("User asked about remembered information.");
                return memoryRecall;
            }
            else
            {
                return "I don't remember anything specific about you yet.";
            }
        }
        else if (previousInput != null && lowerInput.Contains("tell me more about") && ContainsKeyword(previousInput, sentimentAwareResponses.Keys.ToArray()))
        {
            string keyword = GetKeyword(previousInput, sentimentAwareResponses.Keys.ToArray());
            if (sentimentAwareResponses.ContainsKey(keyword) && sentimentAwareResponses[keyword].ContainsKey(Sentiment.Neutral))
            {
                var availableResponses = sentimentAwareResponses[keyword][Sentiment.Neutral].Where(r => r != GetLastResponse(keyword));
                if (availableResponses.Any())
                {
                    string response = availableResponses.ElementAt(new Random().Next(availableResponses.Count()));
                    UpdateLastResponse(keyword, response);
                    LogActivity($"Provided more info on: {keyword}");
                    return response;
                }
                else
                {
                    return $"I've already shared some information about {keyword}. Is there anything else specific you'd like to know?";
                }
            }
        }

        // Sentiment-aware Keyword Responses
        foreach (var keyword in sentimentAwareResponses.Keys)
        {
            // Basic keyword detection (Task 3 NLP Simulation)
            if (lowerInput.Contains(keyword) || (keyword == "online safety" && lowerInput.Contains("safe online")))
            {
                if (sentimentAwareResponses[keyword].ContainsKey(currentSentiment) && sentimentAwareResponses[keyword][currentSentiment].Any())
                {
                    string response = sentimentAwareResponses[keyword][currentSentiment].ElementAt(new Random().Next(sentimentAwareResponses[keyword][currentSentiment].Count()));
                    UpdateLastResponse(keyword, response);
                    LogActivity($"Responded to '{keyword}' with '{currentSentiment}' sentiment.");
                    return response;
                }
                else if (sentimentAwareResponses[keyword].ContainsKey(Sentiment.Neutral) && sentimentAwareResponses[keyword][Sentiment.Neutral].Any())
                {
                    string response = sentimentAwareResponses[keyword][Sentiment.Neutral].ElementAt(new Random().Next(sentimentAwareResponses[keyword][Sentiment.Neutral].Count()));
                    UpdateLastResponse(keyword, response);
                    LogActivity($"Responded to '{keyword}' with 'Neutral' sentiment.");
                    return response;
                }
            }
        }

        if (lowerInput.Contains("give me a phishing tip"))
        {
            string tip = GetRandomPhishingTip();
            UpdateLastResponse("phishing_tip", tip);
            LogActivity("Provided a phishing tip.");
            return tip;
        }

        // Error Handling and Edge Cases
        if (string.IsNullOrWhiteSpace(userInput))
        {
            return "Please enter a question or command.";
        }
        else
        {
            LogActivity($"Did not understand: '{userInput}'");
            return "I didn't quite understand that. Could you rephrase? You can ask me to 'add task', 'view tasks', 'start quiz', or 'show activity log'.";
        }
    }

    private Sentiment DetectSentiment(string text)
    {
        text = text.ToLower();
        if (text.Contains("worried") || text.Contains("concerned") || text.Contains("anxious") || text.Contains("uneasy") || text.Contains("afraid"))
        {
            return Sentiment.Worried;
        }
        else if (text.Contains("curious") || text.Contains("interested to know") || text.Contains("tell me more") || text.Contains("what is") || text.Contains("explain"))
        {
            return Sentiment.Curious;
        }
        else if (text.Contains("frustrated") || text.Contains("confused") || text.Contains("don't understand") || text.Contains("this is hard") || text.Contains("difficult"))
        {
            return Sentiment.Frustrated;
        }
        return Sentiment.Neutral;
    }

    private string GetRandomPhishingTip()
    {
        if (phishingTips.Count > 0)
        {
            return phishingTips[new Random().Next(phishingTips.Count)];
        }
        return "Here's a tip: Be cautious of unsolicited emails.";
    }

    private bool ContainsKeyword(string text, string[] keywords)
    {
        text = text.ToLower();
        return keywords.Any(k => text.Contains(k));
    }

    private string GetKeyword(string text, string[] keywords)
    {
        text = text.ToLower();
        return keywords.FirstOrDefault(k => text.Contains(k));
    }

    private string GetLastResponse(string key)
    {
        return lastResponse.ContainsKey(key) ? lastResponse[key] : null;
    }

    private void UpdateLastResponse(string key, string response)
    {
        lastResponse[key] = response;
    }

    // --- Task 1: Task Assistant Methods ---
    private string AddTask(string userInput)
    {
        // Example: "add task: set up two-factor authentication, remind me in 7 days"
        // Example: "add task: Review privacy settings"
        string lowerInput = userInput.ToLower();
        string taskInfo = "";
        if (lowerInput.Contains("add task:"))
        {
            taskInfo = userInput.Substring(lowerInput.IndexOf("add task:") + "add task:".Length).Trim();
        }
        else if (lowerInput.Contains("create task:") || lowerInput.Contains("set up task:"))
        {
            taskInfo = userInput.Substring(lowerInput.IndexOf("task:") + "task:".Length).Trim();
        }
        else
        {
            // Basic NLP: if just "add task", prompt for details
            LogActivity("Prompted for task details.");
            return "What is the title and description of the task? (e.g., 'Set up 2FA: Enable two-factor authentication for email')";
        }

        string title = "Untitled Task";
        string description = "";
        DateTime? reminderDate = null;

        // Try to parse title and description (simplified NLP)
        int descIndex = taskInfo.IndexOf(": ");
        if (descIndex != -1)
        {
            title = taskInfo.Substring(0, descIndex).Trim();
            description = taskInfo.Substring(descIndex + 2).Trim();
        }
        else
        {
            title = taskInfo; // Assume entire input is title/description for simplicity
            description = taskInfo;
        }

        // Try to parse reminder (simplified NLP)
        if (lowerInput.Contains("remind me in"))
        {
            int daysIndex = lowerInput.IndexOf("remind me in") + "remind me in".Length;
            string daysPart = lowerInput.Substring(daysIndex).Trim();
            int days = 0;
            if (int.TryParse(new string(daysPart.TakeWhile(char.IsDigit).ToArray()), out days))
            {
                reminderDate = DateTime.Today.AddDays(days);
            }
        }
        else if (lowerInput.Contains("remind me on"))
        {
            int dateIndex = lowerInput.IndexOf("remind me on") + "remind me on".Length;
            string datePart = lowerInput.Substring(dateIndex).Trim();
            DateTime parsedDate;
            // Attempt to parse various date formats
            if (DateTime.TryParse(datePart, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                reminderDate = parsedDate;
            }
            else if (DateTime.TryParse(datePart, out parsedDate)) // Fallback to current culture
            {
                reminderDate = parsedDate;
            }
        }
        else if (lowerInput.Contains("tomorrow"))
        {
            reminderDate = DateTime.Today.AddDays(1);
        }
        else if (lowerInput.Contains("next week"))
        {
            reminderDate = DateTime.Today.AddDays(7);
        }

        // Handle direct task input without "add task:" prefix if the user just responds to the prompt
        if (title == "Untitled Task" && !string.IsNullOrWhiteSpace(taskInfo))
        {
            title = taskInfo.Split(':')[0].Trim();
            description = taskInfo; // For simplicity, keep full input as description if no specific desc given
        }

        var newTask = new CybersecurityTask
        {
            Title = title.Replace(":", "").Trim(), // Clean up title if it contains ':'
            Description = description.Replace("remind me in", "").Replace("remind me on", "").Trim(), // Remove reminder phrases from description
            ReminderDate = reminderDate,
            IsCompleted = false
        };

        // Further refine description to remove reminder phrases if they were not removed by prior logic
        if (newTask.Description.Contains("remind me in"))
        {
            newTask.Description = newTask.Description.Substring(0, newTask.Description.IndexOf("remind me in")).Trim();
        }
        if (newTask.Description.Contains("remind me on"))
        {
            newTask.Description = newTask.Description.Substring(0, newTask.Description.IndexOf("remind me on")).Trim();
        }
        if (newTask.Description.Contains("tomorrow"))
        {
            newTask.Description = newTask.Description.Replace("tomorrow", "").Trim();
        }
        if (newTask.Description.Contains("next week"))
        {
            newTask.Description = newTask.Description.Replace("next week", "").Trim();
        }

        cybersecurityTasks.Add(newTask);
        LogActivity($"Task added: '{newTask.Title}'" + (newTask.ReminderDate.HasValue ? $" with reminder on {newTask.ReminderDate.Value.ToShortDateString()}" : ""));
        return $"Task added: '{newTask.Title}'. Description: '{newTask.Description}'" + (reminderDate.HasValue ? $" I will remind you on {reminderDate.Value.ToShortDateString()}." : "");
    }

    private string SetReminderFromFollowUp(string previousInput, string currentInput)
    {
        // This is a very simplified example, assuming the user is immediately responding to a "what task?" prompt
        // and provides a task name and a "remind me in X days"
        string taskTitle = "Unknown Task";
        if (previousInput.Contains("task title")) // Assuming the bot asked for task title
        {
            // This would require more sophisticated NLP to extract the task title from the previous turn
            // For now, let's assume the user just said "remind me in X days"
        }

        int days = 0;
        if (currentInput.Contains("remind me in") && int.TryParse(new string(currentInput.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray()), out days))
        {
            DateTime reminderDate = DateTime.Today.AddDays(days);
            // This is a placeholder. In a real app, you'd associate this reminder with a specific task.
            LogActivity($"Reminder set for '{taskTitle}' in {days} days.");
            return $"Okay, I'll remind you about {taskTitle} in {days} days on {reminderDate.ToShortDateString()}.";
        }
        return "I couldn't set the reminder. Please specify a task and a valid reminder time (e.g., 'remind me in 3 days').";
    }

    private string ViewTasks()
    {
        if (!cybersecurityTasks.Any())
        {
            return "You have no cybersecurity tasks recorded.";
        }

        Console.WriteLine("\n--- Your Cybersecurity Tasks ---");
        for (int i = 0; i < cybersecurityTasks.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {cybersecurityTasks[i]}");
        }
        Console.WriteLine("--------------------------------\n");
        LogActivity("Displayed all tasks.");
        return "Here are your tasks.";
    }

    private string MarkTaskComplete(string userInput)
    {
        string lowerInput = userInput.ToLower();
        int taskNumber;
        if (int.TryParse(new string(lowerInput.Where(char.IsDigit).ToArray()), out taskNumber) && taskNumber > 0 && taskNumber <= cybersecurityTasks.Count)
        {
            cybersecurityTasks[taskNumber - 1].IsCompleted = true;
            LogActivity($"Task '{cybersecurityTasks[taskNumber - 1].Title}' marked as complete.");
            return $"Task '{cybersecurityTasks[taskNumber - 1].Title}' marked as complete!";
        }
        return "Please specify the number of the task you want to mark as complete (e.g., 'complete task 1').";
    }

    private string DeleteTask(string userInput)
    {
        string lowerInput = userInput.ToLower();
        int taskNumber;
        if (int.TryParse(new string(lowerInput.Where(char.IsDigit).ToArray()), out taskNumber) && taskNumber > 0 && taskNumber <= cybersecurityTasks.Count)
        {
            string taskTitle = cybersecurityTasks[taskNumber - 1].Title;
            cybersecurityTasks.RemoveAt(taskNumber - 1);
            LogActivity($"Task '{taskTitle}' deleted.");
            return $"Task '{taskTitle}' deleted.";
        }
        return "Please specify the number of the task you want to delete (e.g., 'delete task 1').";
    }

    // --- Task 2: Quiz Methods ---
    private void InitializeQuizQuestions()
    {
        quizQuestions = new List<QuizQuestion>
        {
            new QuizQuestion(
                "What should you do if you receive an email asking for your password?",
                new List<string> { "A) Reply with your password", "B) Delete the email", "C) Report the email as phishing", "D) Ignore it" },
                2, // C) Report the email as phishing
                "Correct! Reporting phishing emails helps prevent scams. Never share your password."
            ),
            new QuizQuestion(
                "Which of the following is an example of a strong password?",
                new List<string> { "A) Password123", "B) YourName123!", "C) P@$$w0rd!", "D) 2F$aB#9LmP@sW" },
                3, // D) 2F$aB#9LmP@sW
                "Correct! A strong password combines uppercase, lowercase, numbers, and symbols, and is not easily guessable."
            ),
            new QuizQuestion(
                "What is multi-factor authentication (MFA)?",
                new List<string> {
                    "A) Using the same password for multiple accounts",
                    "B) Using two or more verification methods to gain access to a resource",
                    "C) Only using a password to log in",
                    "D) Sharing your login details with a trusted friend"
                },
                1, // B) Using two or more verification methods to gain access to a resource
                "Correct! MFA adds an extra layer of security, making it harder for unauthorized users to access your accounts."
            ),
            new QuizQuestion(
                "What is the primary purpose of antivirus software?",
                new List<string> {
                    "A) To speed up your computer",
                    "B) To protect against physical theft of your device",
                    "C) To detect and remove malicious software (malware)",
                    "D) To block annoying pop-up ads"
                },
                2, // C) To detect and remove malicious software (malware)
                "Correct! Antivirus software is crucial for protecting your system from viruses, spyware, and other malware."
            ),
            new QuizQuestion(
                "Which term refers to a cyber attack that attempts to trick individuals into revealing sensitive information by impersonating a trustworthy entity?",
                new List<string> { "A) DDoS attack", "B) Ransomware", "C) Phishing", "D) Firewall" },
                2, // C) Phishing
                "Correct! Phishing is a common social engineering technique used to steal data."
            ),
            new QuizQuestion(
                "It is generally safe to click on any link sent to you by a friend on social media.",
                new List<string> { "A) True", "B) False" },
                1, // B) False
                "False. Even links from friends can be malicious if their account has been compromised. Always be cautious."
            ),
            new QuizQuestion(
                "What does 'HTTPS' at the beginning of a website address indicate?",
                new List<string> {
                    "A) The website is slow",
                    "B) The website is hosted in Canada",
                    "C) The connection to the website is secure and encrypted",
                    "D) The website is for shopping only"
                },
                2, // C) The connection to the website is secure and encrypted
                "Correct! HTTPS ensures that your communication with the website is protected from eavesdropping and tampering."
            ),
            new QuizQuestion(
                "Which of these should you regularly back up?",
                new List<string> { "A) Important documents", "B) Photos", "C) Videos", "D) All of the above" },
                3, // D) All of the above
                "Correct! Regularly backing up all important data is vital to prevent loss in case of system failure or cyber attack."
            ),
            new QuizQuestion(
                "What is a firewall used for?",
                new List<string> {
                    "A) To cool down your computer",
                    "B) To prevent unauthorized access to or from a private network",
                    "C) To organize your files",
                    "D) To find lost data"
                },
                1, // B) To prevent unauthorized access to or from a private network
                "Correct! A firewall acts as a barrier between your network and the outside world, controlling incoming and outgoing traffic."
            ),
            new QuizQuestion(
                "If you receive an alert that your software is out of date, what should you do?",
                new List<string> {
                    "A) Ignore it, updates are annoying",
                    "B) Update it immediately to patch security vulnerabilities",
                    "C) Uninstall the software",
                    "D) Restart your computer and hope it goes away"
                },
                1, // B) Update it immediately to patch security vulnerabilities
                "Correct! Software updates often include critical security patches that protect you from known vulnerabilities."
            )
        };
        // Shuffle questions
        Random rng = new Random();
        quizQuestions = quizQuestions.OrderBy(a => rng.Next()).ToList();
    }

    private string StartQuiz()
    {
        if (quizQuestions.Count == 0)
        {
            return "I don't have any quiz questions loaded at the moment.";
        }
        inQuizMode = true;
        currentQuizScore = 0;
        currentQuizQuestionIndex = 0;
        LogActivity("Quiz started.");
        return DisplayNextQuizQuestion();
    }

    private string DisplayNextQuizQuestion()
    {
        if (currentQuizQuestionIndex < quizQuestions.Count)
        {
            QuizQuestion q = quizQuestions[currentQuizQuestionIndex];
            Console.WriteLine($"\nQuestion {currentQuizQuestionIndex + 1}/{quizQuestions.Count}:");
            Console.WriteLine(q.Question);
            foreach (var option in q.Options)
            {
                Console.WriteLine(option);
            }
            return "Please enter the letter of your answer (e.g., A, B, C, D):";
        }
        else
        {
            return EndQuiz();
        }
    }

    private void HandleQuizAnswer(string userAnswer)
    {
        if (!inQuizMode) return;

        QuizQuestion q = quizQuestions[currentQuizQuestionIndex];
        string feedback = "";
        bool isCorrect = false;

        if (userAnswer.Length == 1 && char.IsLetter(userAnswer[0]))
        {
            char userChoice = char.ToUpper(userAnswer[0]);
            int chosenIndex = userChoice - 'A';

            if (chosenIndex == q.CorrectOptionIndex)
            {
                currentQuizScore++;
                feedback = $"Correct! {q.Explanation}";
                isCorrect = true;
            }
            else
            {
                feedback = $"Incorrect. The correct answer was {q.Options[q.CorrectOptionIndex][0]}. {q.Explanation}";
            }
        }
        else if (int.TryParse(userAnswer, out int numAnswer) && numAnswer - 1 == q.CorrectOptionIndex)
        {
            currentQuizScore++;
            feedback = $"Correct! {q.Explanation}";
            isCorrect = true;
        }
        else
        {
            feedback = "Invalid input. Please enter the letter (A, B, C, D) or number of your choice.";
        }

        Console.WriteLine($"Chatbot: {feedback}");
        LogActivity($"Quiz answer for Q{currentQuizQuestionIndex + 1}: User answered '{userAnswer}', Correct: {isCorrect}");

        currentQuizQuestionIndex++;
        if (currentQuizQuestionIndex < quizQuestions.Count)
        {
            Console.WriteLine(DisplayNextQuizQuestion());
        }
        else
        {
            Console.WriteLine(EndQuiz());
        }
    }

    private string EndQuiz()
    {
        inQuizMode = false;
        string finalMessage;
        if (currentQuizScore == quizQuestions.Count)
        {
            finalMessage = $"Amazing, {UserName}! You got {currentQuizScore} out of {quizQuestions.Count} questions right! You're a cybersecurity pro!";
        }
        else if (currentQuizScore >= quizQuestions.Count / 2)
        {
            finalMessage = $"Good job, {UserName}! You scored {currentQuizScore} out of {quizQuestions.Count}. Keep learning to stay safe online!";
        }
        else
        {
            finalMessage = $"You scored {currentQuizScore} out of {quizQuestions.Count}, {UserName}. Don't worry, every answer is a learning opportunity. Let's keep working on your cybersecurity knowledge!";
        }
        LogActivity($"Quiz ended. Score: {currentQuizScore}/{quizQuestions.Count}");
        return finalMessage;
    }


    // --- Task 4: Activity Log Methods ---
    private string ShowActivityLog()
    {
        if (!activityLog.Any())
        {
            return "The activity log is empty.";
        }

        Console.WriteLine("\n--- Recent Activity Log ---");
        // Display only the last 5-10 actions (configurable)
        int logDisplayCount = Math.Min(activityLog.Count, 10);
        for (int i = activityLog.Count - logDisplayCount; i < activityLog.Count; i++)
        {
            Console.WriteLine(activityLog[i]);
        }
        Console.WriteLine("---------------------------\n");

        if (activityLog.Count > 10)
        {
            return $"Showing the last {logDisplayCount} activities. There are {activityLog.Count - logDisplayCount} more entries. (Feature: A GUI would have a 'Show more' button)";
        }
        LogActivity("Displayed activity log.");
        return "Here's a summary of recent actions.";
    }


    public static void Main(string[] args)
    {
        InfoGuardCombined infoGuard = new InfoGuardCombined();
        infoGuard.Run();
    }
}
