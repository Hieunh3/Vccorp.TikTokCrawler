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
        /// Insert Content to si_demand_resource_post table (bảng chính)
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
        /// Insert Content to tiktok_link table (bảng local để lưu Link)
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
                    "(post_id,link,domain,status) " +
                    "values (@post_id,@link,@domain,@status)";

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = _conn;
                cmd.CommandText = query;

                cmd.Parameters.AddWithValue("@post_id", content.post_id);
                cmd.Parameters.AddWithValue("@link", content.link);
                cmd.Parameters.AddWithValue("@domain", content.domain);
                cmd.Parameters.AddWithValue("@status", content.status_link);

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
        /// Insert Content to tiktok_source_post table (bảng local để test)
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<int> InserTikTokSourcePostTable(TikTokDTO content)
        {
            int res = 0;

            try
            {
                await _conn.OpenAsync();

                string query = "insert ignore example.tiktok_source_post " +
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
        /// Select URL from tiktok_link table để bóc
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
                                status_link = (int)reader["status"],
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

        /// <summary>
        /// Update status
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<int> UpdateStatus(string link)
        {
            TikTokDTO content = new TikTokDTO();
            int res = 0;
            try
            {
                await _conn.OpenAsync();

                string query = $"UPDATE example.tiktok_link SET status = 1 WHERE link = '{link}'";

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = _conn;
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@status", content.status_link);
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
        /// Select hashtag from si_hashtag table between from date to date
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public List<string> GetHashtagInTableSiHastag(string fromDate,string toDate)
        {
            List<string> data = new List<string>();
            string query = $"Select * from social_index_v2.si_hashtag WHERE create_time BETWEEN '{fromDate}' AND '{toDate}' ";
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(query, _conn))
                {
                    _conn.Open();
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(reader["hashtag"].ToString());
                            
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


    }
}
