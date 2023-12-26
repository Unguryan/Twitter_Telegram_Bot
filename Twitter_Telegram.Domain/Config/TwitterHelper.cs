namespace Twitter_Telegram.Domain.Config
{
    public static class TwitterHelper
    {
        public static string UserInfoByIdUrl => "https://api.twitter.com/1.1/users/show.json?user_id={0}";

        public static string UserInfoByUsernameUrl => "https://api.twitter.com/1.1/users/show.json?screen_name={0}";
        
        public static string UserFriendIdsByUsernameUrl => "https://api.twitter.com/1.1/friends/ids.json?screen_name={0}";


        public static string UserInfoByIdUrlV2 => "https://twitter135.p.rapidapi.com/v1.1/Users/?ids={0}";

        public static string UserInfoByUsernameUrlV2 => "https://twitter135.p.rapidapi.com/v1.1/Users/?usernames={0}";

        public static string UserFriendIdsByUsernameUrlV2 => "https://twitter135.p.rapidapi.com/v1.1/FollowingIds/?username={0}&count={1}";
        public static string UserFriendIdsWithCountByUsernameUrlV2 => "https://twitter135.p.rapidapi.com/v1.1/FollowingIds/?username={0}&count={1}";

    }
}
