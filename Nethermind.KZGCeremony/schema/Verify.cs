namespace Nethermind.KZGCeremony;

public class Verifier
{


    // Schema Check is done in unmarshalling the data from API

    public async Task<bool> Verify()
    {

        Uri url = new Uri("https://seq.ceremony.ethereum.org");
        HttpClient client = new HttpClient();

        client.BaseAddress = url;

        ICoordinator _coordinatorInstance = new Coordinator(client);

        CeremonyState _ceremonyState = _coordinatorInstance.GetTranscript().Result;

        for (int idx = 0; idx < 4; ++idx)
        {
            if (_ceremonyState.transcripts != null)
            {
                SubTranscript st = _ceremonyState.transcripts.ElementAt(idx);
                if (
                    st.witness != null
                    && st.witness.runningProducts != null
                    && st.witness.potPubkeys != null
                )
                {
                    for (int j = 0; j < st.witness.runningProducts.Length - 1; ++j)
                    {
                        string currRunningProduct = st.witness.runningProducts[j];
                        string nextRunningProduct = st.witness.runningProducts[j + 1];
                        string key = st.witness.potPubkeys[j + 1];

                        
                        
                    }
                }
            }
        }

        return true;
    }

}