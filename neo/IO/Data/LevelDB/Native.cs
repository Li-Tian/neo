using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Neo.IO.Data.LevelDB
{
    /// <summary>
    /// leveldb压缩策略
    /// </summary>
    public enum CompressionType : byte
    {
        /// <summary>
        /// 不压缩
        /// </summary>
        kNoCompression = 0x0,
        /// <summary>
        /// kSnappy算法压缩
        /// </summary>
        kSnappyCompression = 0x1
    }

    /// <summary>
    /// Leveldb提供的原生API
    /// </summary>
    public static class Native
    {
#if NET47
        static Native()
        {
            string platform = IntPtr.Size == 8 ? "x64" : "x86";
            LoadLibrary(Path.Combine(AppContext.BaseDirectory, platform, "libleveldb"));
        }

        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary(string dllToLoad);
#endif

        /// <summary>
        /// 创建日志（保留）
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        #region Logger
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_logger_create(IntPtr /* Action<string> */ logger);

        /// <summary>
        /// 释放日志（保留）
        /// </summary>
        /// <param name="option"></param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_logger_destroy(IntPtr /* logger*/ option);
        #endregion

        #region DB
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="options">数据库选项</param>
        /// <param name="name">数据库名字</param>
        /// <param name="error">错误句柄</param>
        /// <returns>数据库句柄</returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_open(IntPtr /* Options*/ options, string name, out IntPtr error);

        /// <summary>
        /// 关闭数据库
        /// </summary>
        /// <param name="db">数据库句柄</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_close(IntPtr /*DB */ db);


        /// <summary>
        /// 添加Key-Value
        /// </summary>
        /// <param name="db">数据库db</param>
        /// <param name="options">写选项</param>
        /// <param name="key">键</param>
        /// <param name="keylen">键长度</param>
        /// <param name="val">值</param>
        /// <param name="vallen">值长度</param>
        /// <param name="errptr">错误处理句柄</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_put(IntPtr /* DB */ db, IntPtr /* WriteOptions*/ options, byte[] key, UIntPtr keylen, byte[] val, UIntPtr vallen, out IntPtr errptr);

        /// <summary>
        /// 删除键
        /// </summary>
        /// <param name="db">数据库db</param>
        /// <param name="options">删除选项</param>
        /// <param name="key">键</param>
        /// <param name="keylen">键长度</param>
        /// <param name="errptr">错误句柄</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_delete(IntPtr /* DB */ db, IntPtr /* WriteOptions*/ options, byte[] key, UIntPtr keylen, out IntPtr errptr);

        /// <summary>
        /// 批量操作
        /// </summary>
        /// <param name="db">数据库db</param>
        /// <param name="options">可选项</param>
        /// <param name="batch">批量操作</param>
        /// <param name="errptr">错误句柄</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_write(IntPtr /* DB */ db, IntPtr /* WriteOptions*/ options, IntPtr /* WriteBatch */ batch, out IntPtr errptr);

        /// <summary>
        /// 查询Key
        /// </summary>
        /// <param name="db">数据库db</param>
        /// <param name="options">可选项</param>
        /// <param name="key">键</param>
        /// <param name="keylen">键长度</param>
        /// <param name="vallen">值长度</param>
        /// <param name="errptr">错误句柄</param>
        /// <returns>值指针</returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_get(IntPtr /* DB */ db, IntPtr /* ReadOptions*/ options, byte[] key, UIntPtr keylen, out UIntPtr vallen, out IntPtr errptr);

        //[DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        //static extern void leveldb_approximate_sizes(IntPtr /* DB */ db, int num_ranges, byte[] range_start_key, long range_start_key_len, byte[] range_limit_key, long range_limit_key_len, out long sizes);

        /// <summary>
        /// 创建迭代器
        /// </summary>
        /// <param name="db">数据库db</param>
        /// <param name="options">可选项</param>
        /// <returns>迭代器指针</returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_create_iterator(IntPtr /* DB */ db, IntPtr /* ReadOption */ options);

        /// <summary>
        /// 创建快照
        /// </summary>
        /// <param name="db">数据库db</param>
        /// <returns>快照句柄</returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_create_snapshot(IntPtr /* DB */ db);

        /// <summary>
        /// 释放快照
        /// </summary>
        /// <param name="db">数据库db</param>
        /// <param name="snapshot">待释放的快照句柄</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_release_snapshot(IntPtr /* DB */ db, IntPtr /* SnapShot*/ snapshot);

        /// <summary>
        /// 获取属性值（保留）
        /// </summary>
        /// <param name="db">数据库db</param>
        /// <param name="propname">属性名</param>
        /// <returns></returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_property_value(IntPtr /* DB */ db, string propname);

        /// <summary>
        /// 修复数据库（保留）
        /// </summary>
        /// <param name="options">可选项</param>
        /// <param name="name">数据库名</param>
        /// <param name="error">错误句柄</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_repair_db(IntPtr /* Options*/ options, string name, out IntPtr error);

        /// <summary>
        /// 销毁数据库（保留）
        /// </summary>
        /// <param name="options">可选项</param>
        /// <param name="name">数据库名</param>
        /// <param name="error">错误句柄</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_destroy_db(IntPtr /* Options*/ options, string name, out IntPtr error);

        #region extensions 

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="ptr">句柄</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_free(IntPtr /* void */ ptr);

        #endregion


        #endregion

        #region Env
        /// <summary>
        /// 创建默认环境变量（保留）
        /// </summary>
        /// <returns></returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_create_default_env();

        /// <summary>
        /// 销毁环境变量（保留）
        /// </summary>
        /// <param name="cache"></param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_env_destroy(IntPtr /*Env*/ cache);
        #endregion

        #region Iterator
        /// <summary>
        /// 销毁迭代器
        /// </summary>
        /// <param name="iterator">待销毁的迭代器</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_iter_destroy(IntPtr /*Iterator*/ iterator);

        /// <summary>
        /// 检查迭代器是否合法
        /// </summary>
        /// <param name="iterator">待检查迭代器</param>
        /// <returns>合法则返回true，否则返回false</returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool leveldb_iter_valid(IntPtr /*Iterator*/ iterator);

        /// <summary>
        /// 移动迭代器游标到首位置
        /// </summary>
        /// <param name="iterator">迭代器</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_iter_seek_to_first(IntPtr /*Iterator*/ iterator);

        /// <summary>
        /// 移动迭代器游标到最后位置
        /// </summary>
        /// <param name="iterator">迭代器</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_iter_seek_to_last(IntPtr /*Iterator*/ iterator);

        /// <summary>
        /// 移动迭代器游标到Key键的位置
        /// </summary>
        /// <param name="iterator">迭代器</param>
        /// <param name="key">被移动到Key</param>
        /// <param name="length">键长度</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_iter_seek(IntPtr /*Iterator*/ iterator, byte[] key, UIntPtr length);

        /// <summary>
        /// 移动迭代器游标到下一个位置
        /// </summary>
        /// <param name="iterator">迭代器</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_iter_next(IntPtr /*Iterator*/ iterator);


        /// <summary>
        /// 移动迭代器游标到上一个位置
        /// </summary>
        /// <param name="iterator">迭代器</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_iter_prev(IntPtr /*Iterator*/ iterator);

        /// <summary>
        /// 查询当前迭代器游标处的键
        /// </summary>
        /// <param name="iterator">迭代器</param>
        /// <param name="length">键长度</param>
        /// <returns>键指针</returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_iter_key(IntPtr /*Iterator*/ iterator, out UIntPtr length);

        /// <summary>
        /// 查询当前迭代器游标处的值
        /// </summary>
        /// <param name="iterator">迭代器</param>
        /// <param name="length">值长度</param>
        /// <returns>值指针</returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_iter_value(IntPtr /*Iterator*/ iterator, out UIntPtr length);

        /// <summary>
        /// 返回迭代器错误句柄
        /// </summary>
        /// <param name="iterator">迭代器</param>
        /// <param name="error">错误句柄</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_iter_get_error(IntPtr /*Iterator*/ iterator, out IntPtr error);
        #endregion

        #region Options
        /// <summary>
        /// 创建可选项
        /// </summary>
        /// <returns>可选项</returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_options_create();

        /// <summary>
        /// 销毁可选项
        /// </summary>
        /// <param name="options">可选项</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_destroy(IntPtr /*Options*/ options);

        /// <summary>
        /// 设置选项：如果数据库不存在则创建
        /// </summary>
        /// <param name="options">待设置的可选项</param>
        /// <param name="o">可选项值</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_create_if_missing(IntPtr /*Options*/ options, [MarshalAs(UnmanagedType.U1)] bool o);

        /// <summary>
        /// 设置选项：若db已存在时返回错误
        /// </summary>
        /// <param name="options">待设置的可选项</param>
        /// <param name="o">可选项值</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_error_if_exists(IntPtr /*Options*/ options, [MarshalAs(UnmanagedType.U1)] bool o);

        /// <summary>
        /// 设置选项：设置info级别日志
        /// </summary>
        /// <param name="options">待设置的可选项</param>
        /// <param name="logger">日志器</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_info_log(IntPtr /*Options*/ options, IntPtr /* Logger */ logger);

        /// <summary>
        /// 设置选项：是否进行数据损坏检查
        /// </summary>
        /// <param name="options">待设置的可选项</param>
        /// <param name="o">可选项值</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_paranoid_checks(IntPtr /*Options*/ options, [MarshalAs(UnmanagedType.U1)] bool o);

        /// <summary>
        /// 设置选项：环境变量（保留）
        /// </summary>
        /// <param name="options">待设置的可选项</param>
        /// <param name="env">环境变量</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_env(IntPtr /*Options*/ options, IntPtr /*Env*/ env);

        /// <summary>
        /// 设置选项 ：写缓存大小
        /// </summary>
        /// <param name="options">待设置的可选项</param>
        /// <param name="size">缓存大小</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_write_buffer_size(IntPtr /*Options*/ options, UIntPtr size);

        /// <summary>
        /// 设置选项：设置打开最大文件数
        /// </summary>
        /// <param name="options">待设置的可选项</param>
        /// <param name="max">最大打开文件数</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_max_open_files(IntPtr /*Options*/ options, int max);

        /// <summary>
        /// 设置选项：设置缓存（保留）
        /// </summary>
        /// <param name="options">待设置的可选项</param>
        /// <param name="cache">缓存</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_cache(IntPtr /*Options*/ options, IntPtr /*Cache*/ cache);
        
        /// <summary>
        /// 设置选项：block块大小
        /// </summary>
        /// <param name="options">待设置的选项</param>
        /// <param name="size">块大小</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_block_size(IntPtr /*Options*/ options, UIntPtr size);

        /// <summary>
        /// 设置选项：重启点
        /// </summary>
        /// <param name="options">待设置的选项</param>
        /// <param name="interval">重启点间隔</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_block_restart_interval(IntPtr /*Options*/ options, int interval);

        /// <summary>
        /// 设置选项：是否进行压缩
        /// </summary>
        /// <param name="options">待设置的选项</param>
        /// <param name="level">压缩级别</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_compression(IntPtr /*Options*/ options, CompressionType level);

        /// <summary>
        /// 设置选项：比较器（保留）
        /// </summary>
        /// <param name="options">待设置的选项</param>
        /// <param name="comparer">比较器</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_comparator(IntPtr /*Options*/ options, IntPtr /*Comparator*/ comparer);

        /// <summary>
        /// 设置选项：过滤器
        /// </summary>
        /// <param name="options">待设置的选项</param>
        /// <param name="policy">过滤策略</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_filter_policy(IntPtr /*Options*/ options, IntPtr /*FilterPolicy*/ policy);
        #endregion

        #region ReadOptions
        /// <summary>
        /// 创建读选项
        /// </summary>
        /// <returns>读选项句柄</returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_readoptions_create();

        /// <summary>
        /// 释放读选项
        /// </summary>
        /// <param name="options">待释放读选项</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_readoptions_destroy(IntPtr /*ReadOptions*/ options);

        /// <summary>
        /// 选项：是否进行数据校验
        /// </summary>
        /// <param name="options">读选项</param>
        /// <param name="o">是否校验</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_readoptions_set_verify_checksums(IntPtr /*ReadOptions*/ options, [MarshalAs(UnmanagedType.U1)] bool o);

        /// <summary>
        /// 选项：是否添加到缓存
        /// </summary>
        /// <param name="options">读选项</param>
        /// <param name="o">是否缓存</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_readoptions_set_fill_cache(IntPtr /*ReadOptions*/ options, [MarshalAs(UnmanagedType.U1)] bool o);

        /// <summary>
        /// 选项：是否从快照读取
        /// </summary>
        /// <param name="options">读选项</param>
        /// <param name="snapshot">快照句柄</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_readoptions_set_snapshot(IntPtr /*ReadOptions*/ options, IntPtr /*SnapShot*/ snapshot);
        #endregion


        #region WriteBatch
        /// <summary>
        /// 批量写选项
        /// </summary>
        /// <returns>批量写句柄</returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_writebatch_create();

        /// <summary>
        /// 销毁批量写句柄
        /// </summary>
        /// <param name="batch">批量写句柄</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_writebatch_destroy(IntPtr /* WriteBatch */ batch);

        /// <summary>
        /// 清空批量写句柄
        /// </summary>
        /// <param name="batch">批量写句柄</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_writebatch_clear(IntPtr /* WriteBatch */ batch);

        /// <summary>
        /// 向批量写中添加键值对
        /// </summary>
        /// <param name="batch">批量写句柄</param>
        /// <param name="key">键</param>
        /// <param name="keylen">键长度</param>
        /// <param name="val">值</param>
        /// <param name="vallen">值长度</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_writebatch_put(IntPtr /* WriteBatch */ batch, byte[] key, UIntPtr keylen, byte[] val, UIntPtr vallen);

        /// <summary>
        /// 从批量写中删除键
        /// </summary>
        /// <param name="batch">批量写句柄</param>
        /// <param name="key">键</param>
        /// <param name="keylen">键长度</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_writebatch_delete(IntPtr /* WriteBatch */ batch, byte[] key, UIntPtr keylen);

        /// <summary>
        /// 批量写迭代器（保留）
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="state"></param>
        /// <param name="put"></param>
        /// <param name="deleted"></param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_writebatch_iterate(IntPtr /* WriteBatch */ batch, object state, Action<object, byte[], int, byte[], int> put, Action<object, byte[], int> deleted);
        #endregion

        #region WriteOptions
        /// <summary>
        /// 创建写选项
        /// </summary>
        /// <returns>写选项句柄</returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_writeoptions_create();

        /// <summary>
        /// 释放写选项
        /// </summary>
        /// <param name="options">待释放写选项</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_writeoptions_destroy(IntPtr /*WriteOptions*/ options);

        /// <summary>
        /// 选项：是否同步到磁盘
        /// </summary>
        /// <param name="options">写选项句柄</param>
        /// <param name="o">是否同步</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_writeoptions_set_sync(IntPtr /*WriteOptions*/ options, [MarshalAs(UnmanagedType.U1)] bool o);
        #endregion

      
        #region Cache 
        /// <summary>
        /// 创建LRU缓存（保留）
        /// </summary>
        /// <param name="capacity">缓存大小</param>
        /// <returns>LRU缓存</returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_cache_create_lru(int capacity);

        /// <summary>
        /// 释放缓存（保留）
        /// </summary>
        /// <param name="cache">待释放的缓存</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_cache_destroy(IntPtr /*Cache*/ cache);
        #endregion

      
        #region Comparator

        /// <summary>
        /// 创建比较器（保留）
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="destructor">构造函数</param>
        /// <param name="compare">比较方法</param>
        /// <param name="name">比较器名字</param>
        /// <returns>比较器</returns>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr /* leveldb_comparator_t* */
            leveldb_comparator_create(
            IntPtr /* void* */ state,
            IntPtr /* void (*)(void*) */ destructor,
            IntPtr
                /* int (*compare)(void*,
                                  const char* a, size_t alen,
                                  const char* b, size_t blen) */
                compare,
            IntPtr /* const char* (*)(void*) */ name);

        /// <summary>
        /// 释放比较器（保留）
        /// </summary>
        /// <param name="cmp">待释放比较器</param>
        [DllImport("libleveldb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_comparator_destroy(IntPtr /* leveldb_comparator_t* */ cmp);

        #endregion
    }

    internal static class NativeHelper
    {
        public static void CheckError(IntPtr error)
        {
            if (error != IntPtr.Zero)
            {
                string message = Marshal.PtrToStringAnsi(error);
                Native.leveldb_free(error);
                throw new LevelDBException(message);
            }
        }
    }
}
