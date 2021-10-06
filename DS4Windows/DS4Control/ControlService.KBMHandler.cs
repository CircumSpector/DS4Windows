using DS4Windows.DS4Control;
using static DS4Windows.Global;

namespace DS4Windows
{
    public partial class ControlService
    {
        public void RefreshOutputKBMHandler()
        {
            if (outputKBMHandler != null)
            {
                outputKBMHandler.Disconnect();
                outputKBMHandler = null;
            }

            if (outputKBMMapping != null) outputKBMMapping = null;

            InitOutputKBMHandler();
        }

        private void InitOutputKBMHandler()
        {
            var attemptVirtualkbmHandler = cmdParser.VirtualKBMHandler;
            Global.InitOutputKBMHandler(attemptVirtualkbmHandler);
            if (!outputKBMHandler.Connect() &&
                attemptVirtualkbmHandler != VirtualKBMFactory.GetFallbackHandlerIdentifier())
            {
                outputKBMHandler = VirtualKBMFactory.GetFallbackHandler();
            }
            else
            {
                // Connection was made. Check if version number should get populated
                if (outputKBMHandler.GetIdentifier() == FakerInputHandler.IDENTIFIER)
                    outputKBMHandler.Version = fakerInputVersion;
            }

            InitOutputKBMMapping(outputKBMHandler.GetIdentifier());
            outputKBMMapping.PopulateConstants();
            outputKBMMapping.PopulateMappings();
        }
    }
}
