using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using VCCorp.TikTokCrawler.Model;

namespace VCCorp.TikTokCrawler.DAO
{
    public class TikTokPostDAO
    {
        private readonly MySqlConnection _conn;
        public TikTokPostDAO(string connection)
        {
            _conn = new MySqlConnection(connection);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                {
                    _conn.Close();
                    _conn.Dispose();
                }
                else
                {
                    _conn.Dispose();
                }
            }
        }

        /// <summary>
        /// Insert Content
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<int> InserToTikTokPostTable(Tiktok_Post_Kafka_Model content)
        {
            int res = 0;

            try
            {
                await _conn.OpenAsync();

                string query = "insert ignore into example.tiktok_post " +
                    "(IdVideo,Username,IdUser,UrlUser,Avatar,Content,LinkVideo,PlayCounts,TimePost,TimePostTimeStamp,TimeCreated,TimeCreateTimeStamp,Followers,Following) " +
                    "values (@IdVideo,@Username,@IdUser,@UrlUser,@Avatar,@Content,@LinkVideo,@PlayCounts,@TimePost,@TimePostTimeStamp,@TimeCreated,@TimeCreateTimeStamp,@Followers,@Following)";

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = _conn;
                cmd.CommandText = query;

                cmd.Parameters.AddWithValue("@IdVideo", content.IdVideo);
                cmd.Parameters.AddWithValue("@Username", content.UserName);
                cmd.Parameters.AddWithValue("@IdUser", content.IdUser);
                cmd.Parameters.AddWithValue("@UrlUser", content.UrlUser);
                cmd.Parameters.AddWithValue("@Avatar", content.Avatar);
                cmd.Parameters.AddWithValue("@Content", content.Content);
                cmd.Parameters.AddWithValue("@LinkVideo", content.LinkVideo);
                cmd.Parameters.AddWithValue("@PlayCounts", content.PlayCounts);
                cmd.Parameters.AddWithValue("@TimePost", content.TimePost);
                cmd.Parameters.AddWithValue("@TimePostTimeStamp", content.TimePostTimeStamp);
                cmd.Parameters.AddWithValue("@TimeCreated", content.TimeCreated);
                cmd.Parameters.AddWithValue("@TimeCreateTimeStamp", content.TimeCreateTimeStamp);
                cmd.Parameters.AddWithValue("@Followers", content.Followers);
                cmd.Parameters.AddWithValue("@Following", content.Following);

                await cmd.ExecuteNonQueryAsync();

                res = 1;

            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("duplicate entry"))
                {
                    res = -2; // trùng link
                }
                else
                {
                    res = -1; // lỗi, bắt lỗi trả ra để sửa

                    // ghi lỗi xuống fil
                }
            }

            return res;
        }


        /// <summary>
        /// Insert Content to si_demand_resource_post table
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<int> InserToSiPostTable(TikTokDTO content)
        {
            int res = 0;

            try
            {
                await _conn.OpenAsync();

                string query = "insert ignore social_index_v2.si_demand_source_post " +
                    "(post_id,platform,link,create_time,update_time,crawled_time,status) " +
                    "values (@post_id,@platform,@link,@create_time,@update_time,@crawled_time,@status)";

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = _conn;
                cmd.CommandText = query;

                cmd.Parameters.AddWithValue("@post_id", content.post_id);
                cmd.Parameters.AddWithValue("@platform", content.platform);
                cmd.Parameters.AddWithValue("@link", content.link);
                cmd.Parameters.AddWithValue("@create_time", content.create_time);
                cmd.Parameters.AddWithValue("@update_time", content.update_time);
                cmd.Parameters.AddWithValue("@crawled_time", content.crawled_time);
                cmd.Parameters.AddWithValue("@status", content.status);


                await cmd.ExecuteNonQueryAsync();

                res = 1;

            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("duplicate entry"))
                {
                    res = -2; // trùng link
                }
                else
                {
                    res = -1; // lỗi, bắt lỗi trả ra để sửa

                    // ghi lỗi xuống fil
                }
            }

            return res;
        }
        /// <summary>
        /// Insert Content to tiktok_link table
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<int> InserTikTokLinkTable(TikTokDTO content)
        {
            int res = 0;

            try
            {
                await _conn.OpenAsync();

                string query = "insert ignore example.tiktok_link " +
                    "(post_id,link,domain) " +
                    "values (@post_id,@link,@domain)";

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = _conn;
                cmd.CommandText = query;

                cmd.Parameters.AddWithValue("@post_id", content.post_id);
                cmd.Parameters.AddWithValue("@link", content.link);
                cmd.Parameters.AddWithValue("@domain", content.domain);

                await cmd.ExecuteNonQueryAsync();

                res = 1;

            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("duplicate entry"))
                {
                    res = -2; // trùng link
                }
                else
                {
                    res = -1; // lỗi, bắt lỗi trả ra để sửa

                    // ghi lỗi xuống fil
                }
            }

            return res;
        }

        /// <summary>
        /// Insert Content to tiktok_url table
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<int> InserTikTokUrlTable(TikTokDTO content)
        {
            int res = 0;

            try
            {
                await _conn.OpenAsync();

                string query = "insert ignore example.tiktok_url " +
                    "(post_id,platform,link,create_time,update_time,crawled_time,domain) " +
                    "values (@post_id,@platform,@link,@create_time,@update_time,@crawled_time,@domain)";

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = _conn;
                cmd.CommandText = query;

                cmd.Parameters.AddWithValue("@post_id", content.post_id);
                cmd.Parameters.AddWithValue("@platform", content.platform);
                cmd.Parameters.AddWithValue("@link", content.link);
                cmd.Parameters.AddWithValue("@create_time", content.create_time);
                cmd.Parameters.AddWithValue("@update_time", content.update_time);
                cmd.Parameters.AddWithValue("@crawled_time", content.crawled_time);
                cmd.Parameters.AddWithValue("@domain", content.domain);


                await cmd.ExecuteNonQueryAsync();

                res = 1;

            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("duplicate entry"))
                {
                    res = -2; // trùng link
                }
                else
                {
                    res = -1; // lỗi, bắt lỗi trả ra để sửa

                    // ghi lỗi xuống fil
                }
            }

            return res;
        }


        /// <summary>
        /// Select URL from tiktok_url table
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public List<TikTokDTO> GetLinkByDomain(string domain)
        {
            List<TikTokDTO> data = new List<TikTokDTO>();
            string query = $"Select * from example.tiktok_link where domain ='{domain}'";
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(query, _conn))
                {
                    _conn.Open();
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new TikTokDTO
                            {
                                link = reader["link"].ToString(),
                            }
                            );
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            _conn.Close();

            return data;
        }
        public async Task TruncateLink_Db()
        {
            await _conn.OpenAsync();
            string query = "TRUNCATE TABLE example.tiktok_link";
            try
            {
                MySqlCommand cmd = new MySqlCommand(query, _conn);
                cmd.ExecuteNonQuery();

                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            _conn.Close();

        }

       

    }
}
