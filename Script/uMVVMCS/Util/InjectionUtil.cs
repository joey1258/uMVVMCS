/*
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

using UnityEngine;
using System;
using uMVVMCS.DIContainer;

namespace uMVVMCS
{
	/// <summary>
	/// 注入工具类
	/// </summary>
	public static class InjectionUtil {
        /// <summary>
        /// Injects into a specified object using container details.
        /// 获取参数 obj 上附着的 InjectFromContainer，用特性的 id 执行注入
        /// </summary>
        public static void Inject(object obj)
        {
            // 获取参数参数的特性
			var attributes = obj.GetType().GetCustomAttributes(true);
			
            // 如果没有获取到，就用空 id 进行注入
			if (attributes.Length == 0) { Inject(obj, null); }
            else
            {
				var containInjectFromContainer = false;
				
				for (var i = 0; i < attributes.Length; i++)
                {
					var attribute = attributes[i];
                    // 如需根据容器的 id 进行匹配，将 InjectFromContainer 的 id 作为要对照的 id 传入
                    if (attribute is InjectFromContainer)
                    {
                        Inject(obj, (attribute as InjectFromContainer).id);
						containInjectFromContainer = true;
					}
				}

                //如果到最后都没有获取到 InjectFromContainer 特性，用空 id 进行注入
                if (!containInjectFromContainer) { Inject(obj, null); }
			}
		}

        /// <summary>
        /// 如果参数 obj 不是单例 binding 的值，为 ContextRoot 的 containersData List 中每个
        /// 元素的 container id 与参数 id 相等的容器注入 obj。
        /// 如参数 id 为空，只要 obj 不是单例就对每一个容器进行注入，否则只对容器 id 相等的容器注入
        /// </summary>
        public static void Inject(object obj, object id)
        {
            // 获取 ContextRoot 中的 containersData List
            var containers = ContextRoot.containersData;

            for (int i = 0; i < containers.Count; i++) {
				var container = containers[i].container;
                // 遍历 list，如果容器 id 不为空且和参数 id 相等，injectOnContainer 为真
                var injectOnContainer = (container.id != null && container.id.Equals(id));

                // 如参数 id 为空或 id 与容器 id 相等，且参数 obj 不与容器中任何 binding 的值重复，
                // 就为当前容器注入 obj(避免重复注入)
                if ((id == null || injectOnContainer) && 
                    !IsExistOnContainer(obj, container))
                {
					container.Inject(obj);
				}
			}
		}
		
		/// <summary>
		/// 返回指定容器中的 object 是否已经存在
		/// </summary>
		public static bool IsExistOnContainer(object obj, IInjectionContainer container)
        {
			var isExist = false;
			var bindings = container.GetBindingsByType(obj.GetType());

            if (bindings == null) { return false; }
			
			for (var i = 0; i < bindings.Count; i++)
            {
                int length = bindings[i].valueList.Count;
                for (int n = 0; n < length; n++)
                {
                    if (bindings[i].valueList[n] == obj)
                    {
                        isExist = true;
                        return isExist;
                    }
                }
			}
			
			return isExist;
        }

        /// <summary>
        /// 返回指定 binder 中的 object 是否已经存在
        /// </summary>
        public static bool IsExistOnBinder(object obj, IBinder binder)
        {
            var isExist = false;
            var bindings = binder.GetBindingsByType(obj.GetType());

            if (bindings == null) { return false; }

            for (var i = 0; i < bindings.Count; i++)
            {
                int length = bindings[i].valueList.Count;
                for (int n = 0; n < length; n++)
                {
                    if (bindings[i].valueList[n] == obj)
                    {
                        isExist = true;
                        return isExist;
                    }
                }
            }

            return isExist;
        }
    }
}