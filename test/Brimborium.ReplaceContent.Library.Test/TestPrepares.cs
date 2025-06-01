using TUnit;
using TUnit.Core;

namespace Brimborium.ReplaceContent.Library;

public partial class TestPrepares
{
    [Test]
    public Task Test000VerifyChecksRun() => VerifyTUnit.VerifyChecks.Run();
}
