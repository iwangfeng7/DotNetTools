using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
namespace FramworkTools
{
    public static class Mapping
    {
        #region 实体映射方案一 通过反射
        /// <summary>
        /// 实体映射-T 映射为 TResult
        /// </summary>
        /// <typeparam name="TResult">目标实体</typeparam>
        /// <param name="source">源数据</param>
        /// <returns></returns>
        public static TResult MapTo<TResult>(object source) where TResult : new()
        {
            TResult result = new TResult();
            foreach (var attribute in result.GetType().GetProperties())
            {
                attribute.SetValue(result, source.GetType().GetProperty(attribute.Name)?.GetValue(source));
            }
            return result;
        }
        #endregion

        #region 实体映射方案二 通过Json反序列化
        /// <summary>
        /// 实体映射 Json反序列化
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TResult JsonMapTo<TResult>(object source)
        {
            return JsonConvert.DeserializeObject<TResult>(JsonConvert.SerializeObject(source));
        }

        #endregion

        #region 实体映射方案三 通过AutoMapper

        public static TResult AutoMapperTo<TResult, T>(T source)
        {
            var configuration = new MapperConfiguration(cfg => cfg.CreateMap<T, TResult>());

            var mapper = configuration.CreateMapper();

            return mapper.Map<TResult>(source);
        }
        #endregion

        #region 实体映射方案四 通过表达式树+字典缓存

        private static Dictionary<string, object> _dic = new Dictionary<string, object>();//缓存字典，缓存后的就是硬编码所以性能高。

        public static TDestination Map<TSource, TDestination>(TSource source)

        {

            string key = $"funckey_{typeof(TSource).FullName}_{typeof(TDestination).FullName}";

            if (!_dic.ContainsKey(key)) //如果该表达式不存在，则走一遍编译过程

            {

                ParameterExpression parameterExpression = Expression.Parameter(typeof(TSource), "p");

                List<MemberBinding> memberBindingList = new List<MemberBinding>();//表示绑定的类派生自的基类，这些绑定用于对新创建对象的成员进行初始化(vs的注解。太生涩了，我这样的小白解释不了，大家将就着看)

                foreach (var item in typeof(TDestination).GetProperties()) //遍历目标类型的所有属性

                {

                    MemberExpression property = Expression.Property(parameterExpression, typeof(TSource).GetProperty(item.Name));//获取到对应的属性

                    MemberBinding memberBinding = Expression.Bind(item, property);//初始化这个属性

                    memberBindingList.Add(memberBinding);

                }

                foreach (var item in typeof(TDestination).GetFields())//遍历目标类型的所有字段

                {

                    MemberExpression property = Expression.Field(parameterExpression, typeof(TSource).GetField(item.Name));//获取到对应的字段

                    MemberBinding memberBinding = Expression.Bind(item, property);//同上

                    memberBindingList.Add(memberBinding);

                }

                MemberInitExpression memberInitExpression = Expression.MemberInit(Expression.New(typeof(TDestination)), memberBindingList.ToArray());//初始化创建新对象

                Expression<Func<TSource, TDestination>> lambda = Expression.Lambda<Func<TSource, TDestination>>(memberInitExpression, parameterExpression);

                _dic[key] = lambda.Compile(); //拼装是一次性的

            }

            return ((Func<TSource, TDestination>)_dic[key]).Invoke(source);

        }


        #endregion
    }
    #region 实体映射方案四 通过表达式树封装===这个有问题

    public static class ExpressionGenericMapper
    {
        private static object func;

        public static TResult Map<TSource, TResult>(TSource source)
        {
            if (func is null)
            {
                ParameterExpression parameterExpression = Expression.Parameter(typeof(TSource), "P");

                var memberBindingList = new List<MemberBinding>();

                foreach (var item in typeof(TResult).GetProperties())//获取目标类型的所有属性
                {
                    MemberExpression property =
                        Expression.Property(parameterExpression, typeof(TSource).GetProperty(item.Name));//获取要绑定的属性

                    MemberBinding memberBinding = Expression.Bind(item, property);//初始化属性
                    memberBindingList.Add(memberBinding);
                }

                foreach (var item in typeof(TResult).GetFields())
                {
                    MemberExpression property =
                        Expression.Field(parameterExpression, typeof(TSource).GetField(item.Name));//找到对应字段
                    MemberBinding memberBinding = Expression.Bind(item, property);
                    memberBindingList.Add(memberBinding);

                }

                MemberInitExpression memberInitExpression =
                    Expression.MemberInit((Expression.New(typeof(TResult))), memberBindingList.ToArray());//初始化创建新对象呢
                Expression<Func<TSource, TResult>> lambdaExpression =
                    Expression.Lambda<Func<TSource, TResult>>(memberInitExpression, parameterExpression);

                func = lambdaExpression.Compile();
            }

            return ((Func<TSource, TResult>)func)(source);
        }
    }

    #endregion
}
