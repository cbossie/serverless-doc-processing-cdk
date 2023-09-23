namespace ServerlessDocProcessing.Constructs;

public class ServerlessDocProcessingStackProps : StackProps
{
    public ServerlessDocProcessingStackProps() :
        base()
    {

    }

    public string EnvironmentName { get; set; } = "dev";
}
