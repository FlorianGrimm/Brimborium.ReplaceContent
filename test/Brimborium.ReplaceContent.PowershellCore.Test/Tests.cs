namespace Brimborium.ReplaceContent;

public partial class Tests {
    [Test, Explicit]
    public async Task Show_ReplaceContent_Example1() => await Verify(this.RunPowershellTest());

    /*
    [Test]
    public async Task Show_ReplaceContent_Example2() => await Verify(this.RunPowershellTest());

    [Test]
    public async Task Show_ReplaceContent_Example3() => await Verify(this.RunPowershellTest());
    */

    [Test]
    public async Task Update_ReplaceContent_Example1() => await Verify(this.RunPowershellTest());

    /*
    [Test]
    public async Task Update_ReplaceContent_Example2() => await Verify(this.RunPowershellTest());

    [Test]
    public async Task Update_ReplaceContent_Example3() => await Verify(this.RunPowershellTest());
    */
}
