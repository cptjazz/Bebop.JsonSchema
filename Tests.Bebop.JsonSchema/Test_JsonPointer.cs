namespace Tests.MyJsonSchema;

public class Test_JsonPointer
{
    [Fact]
    public void Test_Append_Empty()
    {
        var pointer = JsonPointer.Parse("/$defs");
        var newPointer = pointer.AppendPropertyName("").AppendPropertyName("");
        Assert.Equal("/$defs//", newPointer.ToStringWithoutEncoding());

        var pointer2 = JsonPointer.Parse("/$defs");
        var newPointer2 = pointer2
            .AppendPropertyName("")
            .AppendPropertyName("$defs")
            .AppendPropertyName("");
        Assert.Equal("/$defs//$defs/", newPointer2.ToStringWithoutEncoding());
    }
}