namespace JACService.Core.Commands;

public class EchoCommand : TextCommand
{
    public EchoCommand()
    {
        Name = "echo";
        Description = "Repeats the request a specified number of times.";
        Usage = "echo <repetitions> <text>";
        Execute = () =>
        {
            if (string.IsNullOrEmpty(Request)) return false;
            string[] splitRequest = Request.Split(' ');
            if (splitRequest.Length <= 1 || !ushort.TryParse(splitRequest[0], out ushort repetitions)) 
                return false;
            var toRepeat = string.Join(' ', splitRequest.Skip(1));
            Response = string.Join(' ', Enumerable.Repeat(toRepeat, repetitions));

            return true;
        };
    }

    
}