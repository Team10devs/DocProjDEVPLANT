namespace DocProjDEVPLANT.Domain.Entities.Company;

public class Input 
{
    public Input(string key, string type)
    {
        this.key = key;
        this.type = type;
    }

    public string key { get; private set; }
    public string type { get; private set; }
        
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Input other = (Input)obj;
        return key == other.key && type == other.type;
    }
        
    public bool Equals(Input other)
    {
        if (other == null)
            return false;

        return this.key == other.key && this.type == other.type;
    }

}