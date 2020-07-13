namespace Penguin.Api.Playlist
{
    public class PlaylistExecutionSettings
    {
        public bool CopyConfigurations { get; set; }

        public bool CopyHeaders { get; set; }

        public bool ExecuteCustomJavascript { get; set; }

        public string StartId { get; set; }

        public string StopId { get; set; }

        public PlaylistExecutionSettings(bool FirstRun = true)
        {
            this.CopyConfigurations = FirstRun;
            this.ExecuteCustomJavascript = FirstRun;
            this.CopyHeaders = FirstRun;
        }
    }
}