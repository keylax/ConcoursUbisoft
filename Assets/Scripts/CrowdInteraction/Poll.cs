using System.Collections.Generic;

public class Poll
{
    public string room { get; set; }
    public int duration { get; set; }
    public List<PollOption> options { get; set; }

    public Poll(string roomName, int durationInSeconds, List<string> pollOptions)
    {
        room = roomName;
        duration = durationInSeconds;
        options = createPollOptions(pollOptions);
    }

    public JSONObject toJson()
    {
        string pollJson = "";
        pollJson += "{ \n";

        pollJson += "\"roomName\": " + "\"" + room + "\",\n";
        pollJson += "\"duration\": " + duration + ",\n";
        pollJson += "\"options\": " + "[\n";
        for (int i = 0; i < options.Count; i++)
        {
            if (i == options.Count - 1)
            {
                pollJson += "{\"option\": " + "\"" + options[i].option + "\"}\n";
            } else
            {
                pollJson += "{\"option\": " + "\"" + options[i].option + "\"},\n";
            }
        }
        pollJson += "]\n";
        pollJson += "}";

        return JSONObject.Create(pollJson);
    }

    private List<PollOption> createPollOptions(List<string> stringOpts)
    {
        List<PollOption> opts = new List<PollOption>();
        foreach (string opt in stringOpts)
        {
            opts.Add(new PollOption(opt));
        }

        return opts;
    }
}
