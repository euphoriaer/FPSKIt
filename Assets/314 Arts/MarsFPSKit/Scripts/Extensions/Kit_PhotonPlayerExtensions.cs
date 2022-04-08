using Photon.Pun;

namespace MarsFPSKit
{
    public static class Kit_PhotonPlayerExtensions
    {
        public static int GetPlayerScore(this Photon.Realtime.Player p)
        {
            int score = 0;

            //Check for kills
            if (p.CustomProperties["kills"] != null)
            {
                //Add kills to score
                score += (int)p.CustomProperties["kills"];
            }

            return score;
        }

        public static Photon.Realtime.Player Find(int id)
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i].ActorNumber == id) return PhotonNetwork.PlayerList[i];
            }
            return null;
        }
    }
}