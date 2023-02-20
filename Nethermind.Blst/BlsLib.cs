using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using Nethermind.Core.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Nethermind.Logging.TestErrorLogManager;

using size_t = System.UIntPtr;
namespace Nethermind.Blst
{

    public enum BLSERROR
    {
        SUCCESS = 0,
        BAD_ENCODING,
        POINT_NOT_ON_CURVE,
        POINT_NOT_IN_GROUP,
        AGGR_TYPE_MISMATCH,
        VERIFY_FAIL,
        PK_IS_INFINITY,
        BAD_SCALAR,
    }
    public class Exception : ApplicationException
    {
        private readonly BLSERROR code;

        public Exception(BLSERROR err) { code = err; }
        public override string Message
        {
            get
            {
                switch (code)
                {
                    case BLSERROR.BAD_ENCODING: return "bad encoding";
                    case BLSERROR.POINT_NOT_ON_CURVE: return "point not on curve";
                    case BLSERROR.POINT_NOT_IN_GROUP: return "point not in group";
                    case BLSERROR.AGGR_TYPE_MISMATCH: return "aggregate type mismatch";
                    case BLSERROR.VERIFY_FAIL: return "verify failure";
                    case BLSERROR.PK_IS_INFINITY: return "public key is infinity";
                    case BLSERROR.BAD_SCALAR: return "bad scalar";
                    default: return null;
                }
            }
        }
    }


    public static class BlsLib
    {
        //static BlsLib() => AssemblyLoadContext.Default.ResolvingUnmanagedDll += (assembly, path) => NativeLibrary.Load($"runtimes/{(
        //    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
        //    RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" :
        //    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : "")}-{RuntimeInformation.ProcessArchitecture switch
        //    {
        //        Architecture.X64 => "x64",
        //        Architecture.Arm64 => "arm64",
        //        _ => ""
        //    }}/native/{path}.{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dll" : "so")}");

        static BlsLib() => AssemblyLoadContext.Default.ResolvingUnmanagedDll += (assembly, path) => NativeLibrary.Load($"runtimes/{(
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : "")}-{RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.Arm64 => "arm64",
                _ => ""
            }}/native/{path}.{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dll" : "dylib")}");


        private const int P1_COMPRESSED_SZ = 384 / 8;
        private const int P2_COMPRESSED_SZ = 2 * P1_COMPRESSED_SZ;

        [DllImport("blst")]
        static extern size_t blst_p1_affine_sizeof();

        [DllImport("blst")]
        static extern BLSERROR blst_p1_deserialize([Out] long[] ret, [In] byte[] inp);
        [DllImport("blst")]
        static extern void blst_p1_affine_serialize([Out] byte[] ret, [In] long[] inp);
        [DllImport("blst")]
        static extern void blst_p1_affine_compress([Out] byte[] ret, [In] long[] inp);

        [DllImport("blst")]
        static extern void blst_p1_to_affine([Out] long[] ret, [In] long[] inp);
        [DllImport("blst")]
        static extern bool blst_p1_affine_on_curve([In] long[] point);
        [DllImport("blst")]
        static extern bool blst_p1_affine_in_g1([In] long[] point);
        [DllImport("blst")]
        static extern bool blst_p1_affine_is_inf([In] long[] point);
        [DllImport("blst")]
        static extern bool blst_p1_affine_is_equal([In] long[] a, [In] long[] b);

        [DllImport("blst")]
        static extern IntPtr blst_p1_generator();

        [DllImport("blst")]
        static extern BLSERROR blst_core_verify_pk_in_g2([In] long[] pk, [In] long[] sig,
                                                      bool hash_or_encode,
                                                      [In] byte[] msg, size_t msg_len,
                                                      [In] byte[] dst, size_t dst_len,
                                                      [In] byte[] aug, size_t aug_len);

        public static BigInteger FrModulus()
        {
            var modulusBytes = Bytes.FromHexString("73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000001");
            Bytes.ChangeEndianness8(modulusBytes);
            return new BigInteger(modulusBytes);
        }


        public struct P1_Affine
        {
            internal readonly long[] point;

            private static readonly int sz = (int)blst_p1_affine_sizeof() / sizeof(long);

            //public P1_Affine()            { point = new long[sz]; }
            private P1_Affine(bool _) { point = new long[sz]; }
            private P1_Affine(P1_Affine p) { point = (long[])p.point.Clone(); }

            public P1_Affine(byte[] inp) : this(true)
            {
                int len = inp.Length;
                if (len == 0 || len != ((inp[0] & 0x80) == 0x80 ? P1_COMPRESSED_SZ
                                                              : 2 * P1_COMPRESSED_SZ))
                    throw new Exception(BLSERROR.BAD_ENCODING);
                BLSERROR err = blst_p1_deserialize(point, inp);
                if (err != BLSERROR.SUCCESS)
                    throw new Exception(err);
            }
            public P1_Affine(P1 jacobian) : this(true)
            { blst_p1_to_affine(point, jacobian.point); }

            public P1_Affine dup() { return new P1_Affine(this); }
            public P1 to_jacobian() { return new P1(this); }
            public byte[] serialize()
            {
                byte[] ret = new byte[2 * P1_COMPRESSED_SZ];
                blst_p1_affine_serialize(ret, point);
                return ret;
            }
            public byte[] compress()
            {
                byte[] ret = new byte[P1_COMPRESSED_SZ];
                blst_p1_affine_compress(ret, point);
                return ret;
            }

            public bool on_curve() { return blst_p1_affine_on_curve(point); }
            public bool in_group() { return blst_p1_affine_in_g1(point); }
            public bool is_inf() { return blst_p1_affine_is_inf(point); }
            public bool is_equal(P1_Affine p)
            { return blst_p1_affine_is_equal(point, p.point); }

            BLSERROR core_verify(P2_Affine pk, bool hash_or_encode,
                              byte[] msg, string DST = "", byte[] aug = null)
            {
                byte[] dst = Encoding.UTF8.GetBytes(DST);
                return blst_core_verify_pk_in_g2(pk.point, point,
                                                 hash_or_encode,
                                                 msg, (size_t)msg.Length,
                                                 dst, (size_t)dst.Length,
                                                 aug, (size_t)(aug != null ? aug.Length : 0));
            }

            public static P1_Affine generator()
            {
                var ret = new P1_Affine(true);
                Marshal.Copy(blst_p1_generator(), ret.point, 0, ret.point.Length);
                return ret;
            }
        }

        [DllImport("blst")]
        static extern size_t blst_p1_sizeof();
        [DllImport("blst")]
        static extern void blst_p1_serialize([Out] byte[] ret, [In] long[] inp);
        [DllImport("blst")]
        static extern void blst_p1_compress([Out] byte[] ret, [In] long[] inp);
        [DllImport("blst")]
        static extern void blst_p1_from_affine([Out] long[] ret, [In] long[] inp);

        [DllImport("blst")]
        static extern bool blst_p1_on_curve([In] long[] point);
        [DllImport("blst")]
        static extern bool blst_p1_in_g1([In] long[] point);
        [DllImport("blst")]
        static extern bool blst_p1_is_inf([In] long[] point);
        [DllImport("blst")]
        static extern bool blst_p1_is_equal([In] long[] a, [In] long[] b);

        [DllImport("blst")]
        static extern void blst_sk_to_pk_in_g1([Out] long[] ret, [In] byte[] SK);
        [DllImport("blst")]
        static extern
        void blst_encode_to_g1([Out] long[] ret, [In] byte[] msg, size_t msg_len,
                                                 [In] byte[] dst, size_t dst_len,
                                                 [In] byte[] aug, size_t aug_len);
        [DllImport("blst")]
        static extern
        void blst_hash_to_g1([Out] long[] ret, [In] byte[] msg, size_t msg_len,
                                               [In] byte[] dst, size_t dst_len,
                                               [In] byte[] aug, size_t aug_len);
        [DllImport("blst")]
        static extern
        void blst_sign_pk_in_g2([Out] long[] ret, [In] long[] hash, [In] byte[] SK);

        [DllImport("blst")]
        static extern
        void blst_p1_mult([Out] long[] ret, [In] long[] a,
                                            [In] byte[] scalar, size_t nbits);
        [DllImport("blst")]
        static extern void blst_p1_cneg([Out] long[] ret, bool cbit);
        [DllImport("blst")]
        static extern
        void blst_p1_add_or_double([Out] long[] ret, [In] long[] a, [In] long[] b);
        [DllImport("blst")]
        static extern
        void blst_p1_add_or_double_affine([Out] long[] ret, [In] long[] a,
                                                            [In] long[] b);
        [DllImport("blst")]
        static extern void blst_p1_double([Out] long[] ret, [In] long[] a);

        public struct P1
        {
            internal long[] point;

            private static readonly int sz = (int)blst_p1_sizeof() / sizeof(long);

            //public P1()           { point = new long[sz]; }
            private P1(bool _) { point = new long[sz]; }
            private P1(P1 p) { point = (long[])p.point.Clone(); }
            private long[] self()
            { if (point == null) { point = new long[sz]; } return point; }

            public P1(SecretKey sk) : this(true)
            { blst_sk_to_pk_in_g1(point, sk.key); }
            public P1(byte[] inp) : this(true)
            {
                int len = inp.Length;
                if (len == 0 || len != ((inp[0] & 0x80) == 0x80 ? P1_COMPRESSED_SZ
                                                              : 2 * P1_COMPRESSED_SZ))
                    throw new Exception(BLSERROR.BAD_ENCODING);
                BLSERROR err = blst_p1_deserialize(point, inp);
                if (err != BLSERROR.SUCCESS)
                    throw new Exception(err);
                blst_p1_from_affine(point, point);
            }
            public P1(P1_Affine affine) : this(true)
            { blst_p1_from_affine(point, affine.point); }

            public P1 dup() { return new P1(this); }
            public P1_Affine to_affine() { return new P1_Affine(this); }
            public byte[] serialize()
            {
                byte[] ret = new byte[2 * P1_COMPRESSED_SZ];
                blst_p1_serialize(ret, point);
                return ret;
            }
            public byte[] compress()
            {
                byte[] ret = new byte[P1_COMPRESSED_SZ];
                blst_p1_compress(ret, point);
                return ret;
            }

            public bool on_curve() { return blst_p1_on_curve(point); }
            public bool in_group() { return blst_p1_in_g1(point); }
            public bool is_inf() { return blst_p1_is_inf(point); }
            public bool is_equal(P1 p) { return blst_p1_is_equal(point, p.point); }

            public P1 hash_to(byte[] msg, string DST = "", byte[] aug = null)
            {
                byte[] dst = Encoding.UTF8.GetBytes(DST);
                blst_hash_to_g1(self(), msg, (size_t)msg.Length,
                                        dst, (size_t)dst.Length,
                                        aug, (size_t)(aug != null ? aug.Length : 0));
                return this;
            }
            public P1 encode_to(byte[] msg, string DST = "", byte[] aug = null)
            {
                byte[] dst = Encoding.UTF8.GetBytes(DST);
                blst_encode_to_g1(self(), msg, (size_t)msg.Length,
                                          dst, (size_t)dst.Length,
                                          aug, (size_t)(aug != null ? aug.Length : 0));
                return this;
            }

            public P1 sign_with(SecretKey sk)
            { blst_sign_pk_in_g2(point, point, sk.key); return this; }
            public P1 sign_with(Scalar scalar)
            { blst_sign_pk_in_g2(point, point, scalar.val); return this; }

            public void aggregate(P1_Affine inp)
            {
                if (blst_p1_affine_in_g1(inp.point))
                    blst_p1_add_or_double_affine(point, point, inp.point);
                else
                    throw new Exception(BLSERROR.POINT_NOT_IN_GROUP);
            }

            public P1 mult(byte[] scalar)
            {
                blst_p1_mult(point, point, scalar, (size_t)(scalar.Length * 8));
                return this;
            }
            public P1 mult(Scalar scalar)
            {
                blst_p1_mult(point, point, scalar.val, (size_t)255);
                return this;
            }
            public P1 mult(BigInteger scalar)
            {
                byte[] val;
                if (scalar.Sign < 0)
                {
                    val = BigInteger.Negate(scalar).ToByteArray();
                    blst_p1_cneg(point, true);
                }
                else
                {
                    val = scalar.ToByteArray();
                }
                int len = val.Length;
                if (val[len - 1] == 0) len--;
                blst_p1_mult(point, point, val, (size_t)(len * 8));
                return this;
            }
            public P1 cneg(bool flag) { blst_p1_cneg(point, flag); return this; }
            public P1 neg() { blst_p1_cneg(point, true); return this; }
            public P1 add(P1 a)
            { blst_p1_add_or_double(point, point, a.point); return this; }
            public P1 add(P1_Affine a)
            { blst_p1_add_or_double_affine(point, point, a.point); return this; }
            public P1 dbl()
            { blst_p1_double(point, point); return this; }

            public static P1 generator()
            {
                var ret = new P1(true);
                Marshal.Copy(blst_p1_generator(), ret.point, 0, ret.point.Length);
                return ret;
            }
        }

        public static P1 G1() { return P1.generator(); }


        [DllImport("blst")]
        static extern
        void blst_keygen([Out] byte[] key, [In] byte[] IKM, size_t IKM_len,
                                           [In] byte[] info, size_t info_len);
        [DllImport("blst")]
        static extern
        void blst_keygen_v3([Out] byte[] key, [In] byte[] IKM, size_t IKM_len,
                                              [In] byte[] info, size_t info_len);
        [DllImport("blst")]
        static extern
        void blst_keygen_v4_5([Out] byte[] key, [In] byte[] IKM, size_t IKM_len,
                                                [In] byte[] salt, size_t salt_len,
                                                [In] byte[] info, size_t info_len);
        [DllImport("blst")]
        static extern
        void blst_keygen_v5([Out] byte[] key, [In] byte[] IKM, size_t IKM_len,
                                              [In] byte[] salt, size_t salt_len,
                                              [In] byte[] info, size_t info_len);
        [DllImport("blst")]
        static extern void blst_derive_master_eip2333([Out] byte[] key,
                                                      [In] byte[] IKM, size_t IKM_len);
        [DllImport("blst")]
        static extern void blst_derive_child_eip2333([Out] byte[] key,
                                                     [In] byte[] master, uint child_index);

        [DllImport("blst")]
        static extern void blst_scalar_from_bendian([Out] byte[] ret, [In] byte[] key);
        [DllImport("blst")]
        static extern void blst_bendian_from_scalar([Out] byte[] ret, [In] byte[] key);
        [DllImport("blst")]
        static extern bool blst_sk_check([In] byte[] key);

        [DllImport("blst")]
        static extern void blst_scalar_from_lendian([Out] byte[] key, [In] byte[] inp);
        [DllImport("blst")]
        static extern void blst_lendian_from_scalar([Out] byte[] key, [In] byte[] inp);

        public enum ByteOrder
        {
            BigEndian,
            LittleEndian,
        }

        public struct SecretKey
        {
            internal byte[] key;

            //public SecretKey() { key = new byte[32]; }
            public SecretKey(byte[] IKM, string info)
            { key = new byte[32]; keygen(IKM, info); }
            public SecretKey(byte[] inp, ByteOrder order = ByteOrder.BigEndian)
            {
                key = new byte[32];
                switch (order)
                {
                    case ByteOrder.BigEndian: from_bendian(inp); break;
                    case ByteOrder.LittleEndian: from_lendian(inp); break;
                }
            }

            public void keygen(byte[] IKM, string info = "")
            {
                if (key == null) key = new byte[32];
                byte[] info_bytes = Encoding.UTF8.GetBytes(info);
                blst_keygen(key, IKM, (size_t)IKM.Length,
                                 info_bytes, (size_t)info_bytes.Length);
            }
            public void keygen_v3(byte[] IKM, string info = "")
            {
                if (key == null) key = new byte[32];
                byte[] info_bytes = Encoding.UTF8.GetBytes(info);
                blst_keygen_v3(key, IKM, (size_t)IKM.Length,
                                    info_bytes, (size_t)info_bytes.Length);
            }
            public void keygen_v4_5(byte[] IKM, string salt, string info = "")
            {
                if (key == null) key = new byte[32];
                byte[] salt_bytes = Encoding.UTF8.GetBytes(salt);
                byte[] info_bytes = Encoding.UTF8.GetBytes(info);
                blst_keygen_v4_5(key, IKM, (size_t)IKM.Length,
                                      salt_bytes, (size_t)salt_bytes.Length,
                                      info_bytes, (size_t)info_bytes.Length);
            }
            public void keygen_v5(byte[] IKM, byte[] salt, string info = "")
            {
                if (key == null) key = new byte[32];
                byte[] info_bytes = Encoding.UTF8.GetBytes(info);
                blst_keygen_v5(key, IKM, (size_t)IKM.Length,
                                    salt, (size_t)salt.Length,
                                    info_bytes, (size_t)info_bytes.Length);
            }
            public void keygen_v5(byte[] IKM, string salt, string info = "")
            { keygen_v5(IKM, Encoding.UTF8.GetBytes(salt), info); }
            public void derive_master_eip2333(byte[] IKM)
            {
                if (key == null) key = new byte[32];
                blst_derive_master_eip2333(key, IKM, (size_t)IKM.Length);
            }
            public SecretKey(SecretKey master, uint child_index)
            {
                key = new byte[32];
                blst_derive_child_eip2333(key, master.key, child_index);
            }

            public void from_bendian(byte[] inp)
            {
                if (inp.Length != 32)
                    throw new Exception(BLSERROR.BAD_ENCODING);
                if (key == null) key = new byte[32];
                blst_scalar_from_bendian(key, inp);
                if (!blst_sk_check(key))
                    throw new Exception(BLSERROR.BAD_ENCODING);
            }
            public void from_lendian(byte[] inp)
            {
                if (inp.Length != 32)
                    throw new Exception(BLSERROR.BAD_ENCODING);
                if (key == null) key = new byte[32];
                blst_scalar_from_lendian(key, inp);
                if (!blst_sk_check(key))
                    throw new Exception(BLSERROR.BAD_ENCODING);
            }

            public byte[] to_bendian()
            {
                byte[] ret = new byte[32];
                blst_bendian_from_scalar(ret, key);
                return ret;
            }
            public byte[] to_lendian()
            {
                byte[] ret = new byte[32];
                blst_lendian_from_scalar(ret, key);
                return ret;
            }
        }



        [DllImport("blst")]
        static extern void blst_scalar_from_be_bytes([Out] byte[] ret, [In] byte[] inp,
                                                                       size_t inp_len);
        [DllImport("blst")]
        static extern void blst_scalar_from_le_bytes([Out] byte[] ret, [In] byte[] inp,
                                                                       size_t inp_len);
        [DllImport("blst")]
        static extern bool blst_sk_add_n_check([Out] byte[] ret, [In] byte[] a,
                                                                 [In] byte[] b);
        [DllImport("blst")]
        static extern bool blst_sk_sub_n_check([Out] byte[] ret, [In] byte[] a,
                                                                 [In] byte[] b);
        [DllImport("blst")]
        static extern bool blst_sk_mul_n_check([Out] byte[] ret, [In] byte[] a,
                                                                 [In] byte[] b);
        [DllImport("blst")]
        static extern void blst_sk_inverse([Out] byte[] ret, [In] byte[] a);

        public struct Scalar
        {
            internal byte[] val;

            //public Scalar() { val = new byte[32]; }
            public Scalar(byte[] inp, ByteOrder order = ByteOrder.BigEndian)
            {
                val = new byte[32];
                switch (order)
                {
                    case ByteOrder.BigEndian: from_bendian(inp); break;
                    case ByteOrder.LittleEndian: from_lendian(inp); break;
                }
            }
            private Scalar(bool _) { val = new byte[32]; }
            private Scalar(Scalar orig) { val = (byte[])orig.val.Clone(); }

            public Scalar dup() { return new Scalar(this); }

            public void from_bendian(byte[] inp)
            {
                if (val == null) val = new byte[32];
                blst_scalar_from_be_bytes(val, inp, (size_t)inp.Length);
            }
            public void from_lendian(byte[] inp)
            {
                if (val == null) val = new byte[32];
                blst_scalar_from_le_bytes(val, inp, (size_t)inp.Length);
            }

            public byte[] to_bendian()
            {
                byte[] ret = new byte[32];
                blst_bendian_from_scalar(ret, val);
                return ret;
            }
            public byte[] to_lendian()
            {
                byte[] ret = new byte[32];
                blst_lendian_from_scalar(ret, val);
                return ret;
            }

            public Scalar add(SecretKey a)
            {
                if (!blst_sk_add_n_check(val, val, a.key))
                    throw new Exception(BLSERROR.BAD_SCALAR);
                return this;
            }
            public Scalar add(Scalar a)
            {
                if (!blst_sk_add_n_check(val, val, a.val))
                    throw new Exception(BLSERROR.BAD_SCALAR);
                return this;
            }
            public Scalar sub(Scalar a)
            {
                if (!blst_sk_sub_n_check(val, val, a.val))
                    throw new Exception(BLSERROR.BAD_SCALAR);
                return this;
            }
            public Scalar mul(Scalar a)
            {
                if (!blst_sk_mul_n_check(val, val, a.val))
                    throw new Exception(BLSERROR.BAD_SCALAR);
                return this;
            }
            public Scalar inverse()
            { blst_sk_inverse(val, val); return this; }

            public static Scalar operator +(Scalar a, Scalar b)
            { return a.dup().add(b); }
            public static Scalar operator -(Scalar a, Scalar b)
            { return a.dup().sub(b); }
            public static Scalar operator *(Scalar a, Scalar b)
            { return a.dup().mul(b); }
            public static Scalar operator /(Scalar a, Scalar b)
            { return b.dup().inverse().mul(a); }
        }



        [DllImport("blst")]
        static extern size_t blst_p2_sizeof();

        [DllImport("blst")]
        static extern size_t blst_p2_affine_sizeof();

        [DllImport("blst")]
        static extern BLSERROR blst_p2_deserialize([Out] long[] ret, [In] byte[] inp);

        [DllImport("blst")]
        static extern void blst_p2_affine_serialize([Out] byte[] ret, [In] long[] inp);

        [DllImport("blst")]
        static extern IntPtr blst_p2_generator();

        [DllImport("blst")]
        static extern BLSERROR blst_core_verify_pk_in_g1([In] long[] pk, [In] long[] sig,
                                                      bool hash_or_encode,
                                                      [In] byte[] msg, size_t msg_len,
                                                      [In] byte[] dst, size_t dst_len,
                                                      [In] byte[] aug, size_t aug_len);

        [DllImport("blst")]
        static extern void blst_p2_affine_compress([Out] byte[] ret, [In] long[] inp);

        [DllImport("blst")]
        private static extern BLSERROR blst_p1_uncompress([Out] byte[] ret, [In] byte[] input);

        [DllImport("blst")]
        private static extern BLSERROR blst_p2_uncompress([Out] byte[] ret, [In] byte[] input);

        [DllImport("blst")]
        static extern void blst_p2_to_affine([Out] long[] ret, [In] long[] inp);

        [DllImport("blst")]
        static extern bool blst_p2_affine_on_curve([In] long[] point);

        [DllImport("blst")]
        static extern bool blst_p2_affine_in_g2([In] long[] point);

        [DllImport("blst")]
        static extern bool blst_p2_affine_is_inf([In] long[] point);

        [DllImport("blst")]
        static extern bool blst_p2_affine_is_equal([In] long[] a, [In] long[] b);



        [DllImport("blst")]
        static extern void blst_p2_serialize([Out] byte[] ret, [In] long[] inp);
        [DllImport("blst")]
        static extern void blst_p2_compress([Out] byte[] ret, [In] long[] inp);
        [DllImport("blst")]
        static extern void blst_p2_from_affine([Out] long[] ret, [In] long[] inp);

        [DllImport("blst")]
        static extern bool blst_p2_on_curve([In] long[] point);
        [DllImport("blst")]
        static extern bool blst_p2_in_g2([In] long[] point);
        [DllImport("blst")]
        static extern bool blst_p2_is_inf([In] long[] point);
        [DllImport("blst")]
        static extern bool blst_p2_is_equal([In] long[] a, [In] long[] b);

        [DllImport("blst")]
        static extern void blst_sk_to_pk_in_g2([Out] long[] ret, [In] byte[] SK);
        [DllImport("blst")]
        static extern
        void blst_encode_to_g2([Out] long[] ret, [In] byte[] msg, size_t msg_len,
                                                 [In] byte[] dst, size_t dst_len,
                                                 [In] byte[] aug, size_t aug_len);
        [DllImport("blst")]
        static extern
        void blst_hash_to_g2([Out] long[] ret, [In] byte[] msg, size_t msg_len,
                                               [In] byte[] dst, size_t dst_len,
                                               [In] byte[] aug, size_t aug_len);
        [DllImport("blst")]
        static extern
        void blst_sign_pk_in_g1([Out] long[] ret, [In] long[] hash, [In] byte[] SK);

        [DllImport("blst")]
        static extern
        void blst_p2_mult([Out] long[] ret, [In] long[] a,
                                            [In] byte[] scalar, size_t nbits);
        [DllImport("blst")]
        static extern void blst_p2_cneg([Out] long[] ret, bool cbit);
        [DllImport("blst")]
        static extern
        void blst_p2_add_or_double([Out] long[] ret, [In] long[] a, [In] long[] b);
        [DllImport("blst")]
        static extern
        void blst_p2_add_or_double_affine([Out] long[] ret, [In] long[] a,
                                                            [In] long[] b);
        [DllImport("blst")]
        static extern void blst_p2_double([Out] long[] ret, [In] long[] a);

        public struct P2_Affine
        {
            internal readonly long[] point;

            private static readonly int sz = (int)blst_p2_affine_sizeof() / sizeof(long);

            //public P2_Affine()            { point = new long[sz]; }
            private P2_Affine(bool _) { point = new long[sz]; }
            private P2_Affine(P2_Affine p) { point = (long[])p.point.Clone(); }

            public P2_Affine(byte[] inp) : this(true)
            {
                int len = inp.Length;
                if (len == 0 || len != ((inp[0] & 0x80) == 0x80 ? P2_COMPRESSED_SZ
                                                              : 2 * P2_COMPRESSED_SZ))
                    throw new Exception(BLSERROR.BAD_ENCODING);
                BLSERROR err = blst_p2_deserialize(point, inp);
                if (err != BLSERROR.SUCCESS)
                    throw new Exception(err);
            }
            public P2_Affine(P2 jacobian) : this(true)
            { blst_p2_to_affine(point, jacobian.point); }

            public P2_Affine dup() { return new P2_Affine(this); }
            public P2 to_jacobian() { return new P2(this); }
            public byte[] serialize()
            {
                byte[] ret = new byte[2 * P2_COMPRESSED_SZ];
                blst_p2_affine_serialize(ret, point);
                return ret;
            }
            public byte[] compress()
            {
                byte[] ret = new byte[P2_COMPRESSED_SZ];
                blst_p2_affine_compress(ret, point);
                return ret;
            }

            public bool on_curve() { return blst_p2_affine_on_curve(point); }
            public bool in_group() { return blst_p2_affine_in_g2(point); }
            public bool is_inf() { return blst_p2_affine_is_inf(point); }
            public bool is_equal(P2_Affine p)
            { return blst_p2_affine_is_equal(point, p.point); }

            BLSERROR core_verify(P1_Affine pk, bool hash_or_encode,
                              byte[] msg, string DST = "", byte[] aug = null)
            {
                byte[] dst = Encoding.UTF8.GetBytes(DST);
                return blst_core_verify_pk_in_g1(pk.point, point,
                                                 hash_or_encode,
                                                 msg, (size_t)msg.Length,
                                                 dst, (size_t)dst.Length,
                                                 aug, (size_t)(aug != null ? aug.Length : 0));
            }

            public static P2_Affine generator()
            {
                var ret = new P2_Affine(true);
                Marshal.Copy(blst_p2_generator(), ret.point, 0, ret.point.Length);
                return ret;
            }
        }

        public struct P2
        {
            internal long[] point;

            private static readonly int sz = (int)blst_p2_sizeof() / sizeof(long);

            //public P2()           { point = new long[sz]; }
            private P2(bool _) { point = new long[sz]; }
            private P2(P2 p) { point = (long[])p.point.Clone(); }
            private long[] self()
            { if (point == null) { point = new long[sz]; } return point; }

            public P2(SecretKey sk) : this(true)
            { blst_sk_to_pk_in_g2(point, sk.key); }
            public P2(byte[] inp) : this(true)
            {
                int len = inp.Length;
                if (len == 0 || len != ((inp[0] & 0x80) == 0x80 ? P2_COMPRESSED_SZ
                                                              : 2 * P2_COMPRESSED_SZ))
                    throw new Exception(BLSERROR.BAD_ENCODING);
                BLSERROR err = blst_p2_deserialize(point, inp);
                if (err != BLSERROR.SUCCESS)
                    throw new Exception(err);
                blst_p2_from_affine(point, point);
            }
            public P2(P2_Affine affine) : this(true)
            { blst_p2_from_affine(point, affine.point); }

            public P2 dup() { return new P2(this); }
            public P2_Affine to_affine() { return new P2_Affine(this); }
            public byte[] serialize()
            {
                byte[] ret = new byte[2 * P2_COMPRESSED_SZ];
                blst_p2_serialize(ret, point);
                return ret;
            }
            public byte[] compress()
            {
                byte[] ret = new byte[P2_COMPRESSED_SZ];
                blst_p2_compress(ret, point);
                return ret;
            }

            public bool on_curve() { return blst_p2_on_curve(point); }
            public bool in_group() { return blst_p2_in_g2(point); }
            public bool is_inf() { return blst_p2_is_inf(point); }
            public bool is_equal(P2 p) { return blst_p2_is_equal(point, p.point); }

            public P2 hash_to(byte[] msg, string DST = "", byte[] aug = null)
            {
                byte[] dst = Encoding.UTF8.GetBytes(DST);
                blst_hash_to_g2(self(), msg, (size_t)msg.Length,
                                        dst, (size_t)dst.Length,
                                        aug, (size_t)(aug != null ? aug.Length : 0));
                return this;
            }
            public P2 encode_to(byte[] msg, string DST = "", byte[] aug = null)
            {
                byte[] dst = Encoding.UTF8.GetBytes(DST);
                blst_encode_to_g2(self(), msg, (size_t)msg.Length,
                                          dst, (size_t)dst.Length,
                                          aug, (size_t)(aug != null ? aug.Length : 0));
                return this;
            }

            public P2 sign_with(SecretKey sk)
            { blst_sign_pk_in_g1(point, point, sk.key); return this; }
            public P2 sign_with(Scalar scalar)
            { blst_sign_pk_in_g1(point, point, scalar.val); return this; }

            public void aggregate(P2_Affine inp)
            {
                if (blst_p2_affine_in_g2(inp.point))
                    blst_p2_add_or_double_affine(point, point, inp.point);
                else
                    throw new Exception(BLSERROR.POINT_NOT_IN_GROUP);
            }

            public P2 mult(byte[] scalar)
            {
                blst_p2_mult(point, point, scalar, (size_t)(scalar.Length * 8));
                return this;
            }
            public P2 mult(Scalar scalar)
            {
                blst_p2_mult(point, point, scalar.val, (size_t)255);
                return this;
            }
            public P2 mult(BigInteger scalar)
            {
                byte[] val;
                if (scalar.Sign < 0)
                {
                    val = BigInteger.Negate(scalar).ToByteArray();
                    blst_p2_cneg(point, true);
                }
                else
                {
                    val = scalar.ToByteArray();
                }
                int len = val.Length;
                if (val[len - 1] == 0) len--;
                blst_p2_mult(point, point, val, (size_t)(len * 8));
                return this;
            }
            public P2 cneg(bool flag) { blst_p2_cneg(point, flag); return this; }
            public P2 neg() { blst_p2_cneg(point, true); return this; }
            public P2 add(P2 a)
            { blst_p2_add_or_double(point, point, a.point); return this; }
            public P2 add(P2_Affine a)
            { blst_p2_add_or_double_affine(point, point, a.point); return this; }
            public P2 dbl()
            { blst_p2_double(point, point); return this; }

            public static P2 generator()
            {
                var ret = new P2(true);
                Marshal.Copy(blst_p2_generator(), ret.point, 0, ret.point.Length);
                return ret;
            }
        }

        public static P2 G2() { return P2.generator(); }


        [DllImport("blst")]
        static extern void blst_aggregated_in_g1([Out] long[] fp12, [In] long[] p);
        [DllImport("blst")]
        static extern void blst_aggregated_in_g2([Out] long[] fp12, [In] long[] p);
        [DllImport("blst")]
        static extern BLSERROR blst_pairing_aggregate_pk_in_g2([In, Out] long[] fp12,
                                        [In] long[] pk, [In] long[] sig,
                                        [In] byte[] msg, size_t msg_len,
                                        [In] byte[] aug, size_t aug_len);
        [DllImport("blst")]
        static extern BLSERROR blst_pairing_mul_n_aggregate_pk_in_g2([In, Out] long[] fp12,
                                        [In] long[] pk, [In] long[] sig,
                                        [In] byte[] scalar, size_t nbits,
                                        [In] byte[] msg, size_t msg_len,
                                        [In] byte[] aug, size_t aug_len);


        [DllImport("blst")]
        static extern size_t blst_fp12_sizeof();
        [DllImport("blst")]
        static extern void blst_miller_loop([Out] long[] fp12, [In] long[] q,
                                                               [In] long[] p);
        [DllImport("blst")]
        static extern bool blst_fp12_is_one([In] long[] fp12);
        [DllImport("blst")]
        static extern bool blst_fp12_is_equal([In] long[] a, [In] long[] b);
        [DllImport("blst")]
        static extern void blst_fp12_sqr([Out] long[] ret, [In] long[] a);
        [DllImport("blst")]
        static extern void blst_fp12_mul([Out] long[] ret, [In] long[] a,
                                                           [In] long[] b);
        [DllImport("blst")]
        static extern void blst_final_exp([Out] long[] ret, [In] long[] a);
        [DllImport("blst")]
        static extern bool blst_fp12_finalverify([In] long[] a, [In] long[] b);
        [DllImport("blst")]
        static extern IntPtr blst_fp12_one();
        [DllImport("blst")]
        static extern bool blst_fp12_in_group([In] long[] a);
        [DllImport("blst")]
        static extern void blst_bendian_from_fp12([Out] byte[] ret, [In] long[] a);

        public struct PT
        {
            internal readonly long[] fp12;

            private static readonly int sz = (int)blst_fp12_sizeof() / sizeof(long);

            internal PT(bool _) { fp12 = new long[sz]; }
            private PT(PT orig) { fp12 = (long[])orig.fp12.Clone(); }

            public PT(P1_Affine p) : this(true)
            { blst_aggregated_in_g1(fp12, p.point); }
            public PT(P1 p) : this(true)
            { blst_aggregated_in_g1(fp12, (new P1_Affine(p)).point); }
            public PT(P2_Affine q) : this(true)
            { blst_aggregated_in_g2(fp12, q.point); }
            public PT(P2 q) : this(true)
            { blst_aggregated_in_g2(fp12, (new P2_Affine(q)).point); }
            public PT(P2_Affine q, P1_Affine p) : this(true)
            { blst_miller_loop(fp12, q.point, p.point); }
            public PT(P1_Affine p, P2_Affine q) : this(q, p) { }
            public PT(P2 q, P1 p) : this(true)
            {
                blst_miller_loop(fp12, (new P2_Affine(q)).point,
                                       (new P1_Affine(p)).point);
            }
            public PT(P1 p, P2 q) : this(q, p) { }

            public PT dup() { return new PT(this); }
            public bool is_one() { return blst_fp12_is_one(fp12); }
            public bool is_equal(PT p)
            { return blst_fp12_is_equal(fp12, p.fp12); }
            public PT sqr() { blst_fp12_sqr(fp12, fp12); return this; }
            public PT mul(PT p) { blst_fp12_mul(fp12, fp12, p.fp12); return this; }
            public PT final_exp() { blst_final_exp(fp12, fp12); return this; }
            public bool in_group() { return blst_fp12_in_group(fp12); }
            public byte[] to_bendian()
            {
                byte[] ret = new byte[12 * P1_COMPRESSED_SZ];
                blst_bendian_from_fp12(ret, fp12);
                return ret;
            }

            public static bool finalverify(PT gt1, PT gt2)
            { return blst_fp12_finalverify(gt1.fp12, gt2.fp12); }

            public static PT one()
            {
                var ret = new PT(true);
                Marshal.Copy(blst_fp12_one(), ret.fp12, 0, ret.fp12.Length);
                return ret;
            }
        }

        [DllImport("blst")]
        static extern size_t blst_pairing_sizeof();
        [DllImport("blst")]
        static extern
        void blst_pairing_init([In, Out] long[] ctx, bool hash_or_encode,
                                                     [In] ref long dst, size_t dst_len);
        [DllImport("blst")]
        static extern void blst_pairing_commit([In, Out] long[] ctx);
        [DllImport("blst")]
        static extern BLSERROR blst_pairing_merge([In, Out] long[] ctx, [In] long[] ctx1);
        [DllImport("blst")]
        static extern bool blst_pairing_finalverify([In] long[] ctx, [In] long[] sig);
        [DllImport("blst")]
        static extern
        void blst_pairing_raw_aggregate([In, Out] long[] ctx, [In] long[] q,
                                                              [In] long[] p);
        [DllImport("blst")]
        static extern IntPtr blst_pairing_as_fp12([In] long[] ctx);

        [DllImport("blst")]
        static extern BLSERROR blst_pairing_aggregate_pk_in_g1([In, Out] long[] fp12,
                                        [In] long[] pk, [In] long[] sig,
                                        [In] byte[] msg, size_t msg_len,
                                        [In] byte[] aug, size_t aug_len);
        [DllImport("blst")]
        static extern BLSERROR blst_pairing_mul_n_aggregate_pk_in_g1([In, Out] long[] fp12,
                                        [In] long[] pk, [In] long[] sig,
                                        [In] byte[] scalar, size_t nbits,
                                        [In] byte[] msg, size_t msg_len,
                                        [In] byte[] aug, size_t aug_len);

        public struct Pairing
        {
            private readonly long[] ctx;

            private static readonly int sz = (int)blst_pairing_sizeof() / sizeof(long);

            public Pairing(bool hash_or_encode = false, string DST = "")
            {
                byte[] dst = Encoding.UTF8.GetBytes(DST);
                int dst_len = dst.Length;
                int add_len = dst_len != 0 ? (dst_len + sizeof(long) - 1) / sizeof(long) : 1;
                Array.Resize(ref dst, add_len * sizeof(long));

                ctx = new long[sz + add_len];

                for (int i = 0; i < add_len; i++)
                    ctx[sz + i] = BitConverter.ToInt64(dst, i * sizeof(long));

                GCHandle h = GCHandle.Alloc(ctx, GCHandleType.Pinned);
                blst_pairing_init(ctx, hash_or_encode, ref ctx[sz], (size_t)dst_len);
                h.Free();
            }

            public BLSERROR aggregate(P1_Affine pk, Nullable<P2_Affine> sig,
                                                 byte[] msg, byte[] aug = null)
            {
                return blst_pairing_aggregate_pk_in_g1(ctx, pk.point,
                                        sig.HasValue ? sig.Value.point : null,
                                        msg, (size_t)msg.Length,
                                        aug, (size_t)(aug != null ? aug.Length : 0));
            }
            public BLSERROR aggregate(P2_Affine pk, Nullable<P1_Affine> sig,
                                                 byte[] msg, byte[] aug = null)
            {
                return blst_pairing_aggregate_pk_in_g2(ctx, pk.point,
                                        sig.HasValue ? sig.Value.point : null,
                                        msg, (size_t)msg.Length,
                                        aug, (size_t)(aug != null ? aug.Length : 0));
            }
            public BLSERROR mul_n_aggregate(P2_Affine pk, P1_Affine sig,
                                                       byte[] scalar, int nbits,
                                                       byte[] msg, byte[] aug = null)
            {
                return blst_pairing_mul_n_aggregate_pk_in_g2(ctx, pk.point, sig.point,
                                        scalar, (size_t)nbits,
                                        msg, (size_t)msg.Length,
                                        aug, (size_t)(aug != null ? aug.Length : 0));
            }
            public BLSERROR mul_n_aggregate(P1_Affine pk, P2_Affine sig,
                                                       byte[] scalar, int nbits,
                                                       byte[] msg, byte[] aug = null)
            {
                return blst_pairing_mul_n_aggregate_pk_in_g1(ctx, pk.point, sig.point,
                                        scalar, (size_t)nbits,
                                        msg, (size_t)msg.Length,
                                        aug, (size_t)(aug != null ? aug.Length : 0));
            }

            public void commit() { blst_pairing_commit(ctx); }
            public void merge(Pairing a)
            {
                var err = blst_pairing_merge(ctx, a.ctx);
                if (err != BLSERROR.SUCCESS)
                    throw new Exception(err);
            }
            public bool finalverify(PT sig = new PT())
            { return blst_pairing_finalverify(ctx, sig.fp12); }

            public void raw_aggregate(P2_Affine q, P1_Affine p)
            { blst_pairing_raw_aggregate(ctx, q.point, p.point); }
            public void raw_aggregate(P1_Affine p, P2_Affine q)
            { raw_aggregate(q, p); }
            public void raw_aggregate(P2 q, P1 p)
            {
                blst_pairing_raw_aggregate(ctx, (new P2_Affine(q)).point,
                                                (new P1_Affine(p)).point);
            }
            public void raw_aggregate(P1 p, P2 q)
            { raw_aggregate(q, p); }
            public PT as_fp12()
            {
                var ret = new PT(true);
                GCHandle h = GCHandle.Alloc(ctx, GCHandleType.Pinned);
                Marshal.Copy(blst_pairing_as_fp12(ctx), ret.fp12, 0, ret.fp12.Length);
                h.Free();
                return ret;
            }
        }


        public static byte[] BlsP1Uncompress(byte[] input)
        {
            if (input.Length != 48)
                throw new Exception(BLSERROR.BAD_ENCODING);
            byte[] ret = new byte[96];
            blst_p1_uncompress(ret, input);
            return ret;
        }

        public static byte[] BlsP2Uncompress(byte[] input)
        {
            if (input.Length != 96)
                throw new Exception(BLSERROR.BAD_ENCODING);
            byte[] ret = new byte[192];
            blst_p2_uncompress(ret, input);
            return ret;
        }
    }
}

