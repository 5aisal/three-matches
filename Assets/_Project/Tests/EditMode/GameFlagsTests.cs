using NUnit.Framework;
using ThreeMatches.Core;

namespace ThreeMatches.Tests.EditMode
{
    public class GameFlagsTests
    {
        [Test]
        public void Set_ReturnsTrueOnce_AndRaisesEventOnce()
        {
            var flags = new GameFlags();
            int raised = 0;
            flags.FlagSet += _ => raised++;

            Assert.IsFalse(flags.Has(GameFlags.HasMatches));
            Assert.IsTrue(flags.Set(GameFlags.HasMatches));
            Assert.IsFalse(flags.Set(GameFlags.HasMatches));
            Assert.IsTrue(flags.Has(GameFlags.HasMatches));
            Assert.AreEqual(1, raised);
        }

        [Test]
        public void Set_IgnoresEmptyNames()
        {
            var flags = new GameFlags();
            Assert.IsFalse(flags.Set(""));
            Assert.IsFalse(flags.Set(null));
        }
    }
}
