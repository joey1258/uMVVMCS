﻿/*
 * Copyright 2016 Sun Ning（Joey1258）
 *
 *	Licensed under the Apache License, Version 2.0 (the "License");
 *	you may not use this file except in compliance with the License.
 *	You may obtain a copy of the License at
 *
 *		http://www.apache.org/licenses/LICENSE-2.0
 *
 *		Unless required by applicable law or agreed to in writing, software
 *		distributed under the License is distributed on an "AS IS" BASIS,
 *		WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *		See the License for the specific language governing permissions and
 *		limitations under the License.
 */

using System;
using System.Collections.Generic;

namespace uMVVMCS.DIContainer
{
    public class InjectionContainer : Injector, IInjectionContainer
    {
        /// <summary>
        /// 容器 id
        /// </summary>
        public object id { get; private set; }

        /// <summary>
        /// 容器 AOT 接口 list
        /// </summary>
        private List<IContainerAOT> AOT;

        #region constructor

        public InjectionContainer() : base(new ReflectionCache(), new InjectionBinder())
        {
            RegisterItself();
        }

        public InjectionContainer(object id) : base(new ReflectionCache(), new InjectionBinder())
        {
            this.id = id;
            RegisterItself();
        }

        public InjectionContainer(IReflectionCache cache) : base(cache, new InjectionBinder())
        {
            RegisterItself();
        }

        public InjectionContainer(object id, IReflectionCache cache) : base(cache, new InjectionBinder())
        {
            this.id = id;
            RegisterItself();
        }

        public InjectionContainer(IReflectionCache cache, InjectionBinder binder) : base(cache, binder)
        {
            RegisterItself();
        }

        public InjectionContainer(object id, IReflectionCache cache, InjectionBinder binder) : base(cache, binder)
        {
            this.id = id;
            RegisterItself();
        }

        #endregion

        #region IDisposable implementation 

        /// <summary>
        /// 清空所有缓存和 binder
        /// </summary>
        public void Dispose()
        {
            cache = null;
            binder = null;
        }

        #endregion

        #region IInjectionContainer implementation 

        /// <summary>
        /// 注册容器到 AOT list 
        /// </summary>
        virtual public IInjectionContainer RegisterAOT<T>() where T : IContainerAOT
        {
            RegisterAOT(Resolve<T>());

            return this;
        }

        /// <summary>
        /// 注册容器到 AOT list 
        /// </summary>
        virtual public IInjectionContainer RegisterAOT(IContainerAOT extension)
        {
            // 如果 List<IContainerAOT> AOT 为空,将其初始化
            if (AOT == null) AOT = new List<IContainerAOT>();
            // 添加参数到 list
            AOT.Add(extension);
            // 执行 OnRegister 方法
            extension.OnRegister(this);

            return this;
        }

        /// <summary>
        /// 将所有指定类型的容器从 AOT list 中移除 
        /// </summary>
        virtual public IInjectionContainer UnregisterAOT<T>() where T : IContainerAOT
        {
            // 获取list 中所有指定类型的容器 AOT 接口对象
            var AOTToUnregister = AOT.OfTheType<T, IContainerAOT>();

            // 注销所有获取到的容器 AOT 接口对象
            int length = AOTToUnregister.Count;
            for (int i = 0; i < length; i++)
            {
                UnregisterAOT(AOTToUnregister[i]);
            }

            return this;
        }

        /// <summary>
        /// 将一个容器从 AOT list 中移除 
        /// </summary>
        virtual public IInjectionContainer UnregisterAOT(IContainerAOT aot)
        {
            if (!AOT.Contains(aot)) { return this; }

            AOT.Remove(aot);
            aot.OnUnregister(this);

            return this;
        }

        #endregion

        #region binder AOT event

        virtual public event BindingAddedHandler beforeAddBinding
        {
            add { this.binder.beforeAddBinding += value; }
            remove { this.binder.beforeAddBinding -= value; }
        }

        virtual public event BindingAddedHandler afterAddBinding
        {
            add { this.binder.afterAddBinding += value; }
            remove { this.binder.afterAddBinding -= value; }
        }

        virtual public event BindingRemovedHandler beforeRemoveBinding
        {
            add { this.binder.beforeRemoveBinding += value; }
            remove { this.binder.beforeRemoveBinding -= value; }
        }

        virtual public event BindingRemovedHandler afterRemoveBinding
        {
            add { this.binder.afterRemoveBinding += value; }
            remove { this.binder.afterRemoveBinding -= value; }
        }

        #endregion

        #region binder function

        #region Bind

        /// <summary>
        /// 返回一个新的Binding实例,并设置指定类型给 type, BindingType 为 TEMP，值约束为 MULTIPLE
        /// </summary>
        virtual public IBinding Bind<T>()
        {
            return binder.Bind<T>();
        }

        /// <summary>
        /// 返回一个新的Binding实例，并设置指定类型给 type, BindingType 为 SINGLETON，值约束为 SINGLE
        /// </summary>
        virtual public IBinding BindSingleton<T>()
        {
            return binder.Bind<T>();
        }

        /// <summary>
        /// 返回一个新的Binding实例，并设置指定类型给 type, BindingType 为 FACTORY，值约束为 SINGLE
        /// </summary>
        virtual public IBinding BindFactory<T>()
        {
            return binder.Bind<T>();
        }

        /// <summary>
        /// 返回一个新的Binding实例，并把设置参数分别给 type 和 BindingType，值约束为 SINGLE
        /// </summary>
        virtual public IBinding Bind(Type type, BindingType bindingType)
        {
            return binder.Bind(type, bindingType);
        }

        /// <summary>
        /// 创建多个指定类型的 binding，并返回 IBindingFactory
        /// </summary>
        virtual public IBindingFactory MultipleBind(IList<Type> types, IList<BindingType> bindingTypes)
        {
            return binder.MultipleBind(types, bindingTypes);
        }

        #endregion

        #region GetBinding

        /// <summary>
        /// 根据类型获取 typeBindings 字典和 bindingStorage 中的所有同类型 Binding
        /// </summary>
        virtual public IList<IBinding> GetBindingsByType<T>()
        {
            return binder.GetBindingsByType(typeof(T));
        }

        /// <summary>
        /// 根据类型获取 typeBindings 字典和 bindingStorage 中的所有同类型 Binding
        /// </summary>
        virtual public IList<IBinding> GetBindingsByType(Type type)
        {
            return binder.GetBindingsByType(type);
        }

        /// <summary>
        /// 获取 bindingStorage 中 所有指定 id 的 binding
        /// </summary>
        virtual public IList<IBinding> GetBindingsById(object id)
        {
            return binder.GetBindingsById(id);
        }

        /// <summary>
        /// 获取 binder 中所有的 Binding
        /// </summary>
        virtual public IList<IBinding> GetAllBindings()
        {
            return binder.GetAllBindings();
        }

        /// <summary>
        /// 返回 typeBindings 中除自身以外所有 type 和值都相同的 binding
        /// </summary>
        virtual public IList<IBinding> GetSameNullIdBinding(IBinding binding)
        {
            return binder.GetSameNullIdBinding(binding);
        }

        /// <summary>
        /// 根据类型和id获取 bindingStorage 中的 Binding
        /// </summary>
        virtual public IBinding GetBinding<T>(object id)
        {
            return binder.GetBinding(typeof(T), id);
        }

        /// <summary>
        /// 根据类型和id获取 bindingStorage 中的 Binding
        /// </summary>
        virtual public IBinding GetBinding(Type type, object id)
        {
            return binder.GetBinding(type, id);
        }

        #endregion

        #region Unbind

        /// <summary>
        /// 根据类型从 bindingStorage 和 typeBindings 中删除所有同类型 Binding
        /// </summary>
        virtual public void UnbindByType<T>()
        {
            binder.UnbindByType<T>();
        }

        /// <summary>
        /// 根据类型从 bindingStorage 和 typeBindings 中删除所有同类型 Binding
        /// </summary>
        virtual public void UnbindByType(Type type)
        {
            binder.UnbindByType(type);
        }

        /// <summary>
        /// 根据类型从 typeBindings 中删除所有同类型 Binding
        /// </summary>
        virtual public void UnbindNullIdBindingByType<T>()
        {
            binder.UnbindNullIdBindingByType<T>();
        }

        /// <summary>
        /// 根据类型从 typeBindings 中删除所有同类型 Binding
        /// </summary>
        virtual public void UnbindNullIdBindingByType(Type type)
        {
            binder.UnbindNullIdBindingByType(type);
        }

        /// <summary>
        /// 根据类型和 id 从 bindingStorage 中删除 Binding
        /// </summary>
		virtual public void Unbind<T>(object id)
        {
            binder.Unbind<T>(id);
        }

        /// <summary>
        /// 根据类型和 id 从 bindingStorage 中删除 Binding
        /// </summary>
		virtual public void Unbind(Type type, object id)
        {
            binder.Unbind(type, id);
        }

        /// <summary>
        /// 根据 binding 从 bindingStorage 中删除 Binding
        /// </summary>
        virtual public void Unbind(IBinding binding)
        {
            binder.Unbind(binding);
        }

        #endregion

        #region Remove

        /// <summary>
        /// 删除指定 binding 中指定的 value 值，如果移除后 value 属性为空或 value 约束为唯一，就移除该 binding
        /// </summary>
        virtual public void RemoveValue(IBinding binding, object value)
        {
            binder.RemoveValue(binding, value);
        }

        /// <summary>
        /// 删除指定 binding 中 value 的多个值，如果移除后 value 属性为空或 value 约束为唯一，就移除该 binding
        /// </summary>
        virtual public void RemoveValues(IBinding binding, IList<object> values)
        {
            binder.RemoveValues(binding, values);
        }

        /// <summary>
        /// 删除 binding 自身
        /// </summary>
        virtual public void RemoveBinding(IBinding binding)
        {
            binder.RemoveBinding(binding);
        }

        /// <summary>
        /// 根据 type 和 id 删除 binding （type 和 id 不可为空）
        /// </summary>
        virtual public void RemoveBinding(Type type, object id)
        {
            binder.RemoveBinding(type, id);
        }

        #endregion

        /// <summary>
        /// 储存 binding
        /// </summary>
        public void Storing(IBinding binding)
        {
            binder.Storing(binding);
        }

        #endregion

        /// <summary>
        /// 绑定一个单例 binding，其 type 为自身类型，自身作为实例保存到 value
        /// </summary>
        virtual protected void RegisterItself()
        {
            BindSingleton<IInjectionContainer>().To(this);
        }

    }
}