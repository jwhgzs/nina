# Nina的自我介绍
Hello! 我就是实习时长一个月的实习生——Nina！
这是我的简历：
- 姓名： Nina Script
- 性别： 0
- 生日： 12月24日
- 特长： 跑脚本业务
- 性格： 温柔、细心、幽默
- 缺点： 与同行相比，办事有点慢

# Nina的姐妹花
Nina的语法、特性大部分参照了JavaScript——她们俩的语法移植可以说是无障碍的了！

不过请先别急——来看看这段Nina代码：
```
// 定义 fibo 函数
func fibo(n) {
    if (n == 1 || n == 2) {
        return 1;
    }
    // self 变量指代当前函数
    return self(n - 1) + self(n - 2);
}

// 保存当前时间戳以计时
var t1 = time_now();
// 输出结果和耗时
console_printf('fibo(33) = ' + fibo(33));
console_printf('time: ' + (time_now() - t1) * 1000 + ' ms');
```
这是一段计算斐波那契数的Nina代码。事实上，如果把关键字`func`改为`function`，这就是一段正经的的JavaScript代码！

另外，Nina的几个基本属性：
- 动态语言（在需要运行时才进行解释或编译）
- 动态类型语言（类型在运行时确定和更改）
- 弱类型语言（可隐式、自由地进行类型转换）

这几点也与JavaScript完全一致！看来这对姐妹花是亲生的没错了。

# 进一步了解Nina
由于Nina懒得写更多的自我介绍，你可以翻看项目文件中的[examples](https://github.com/jwhgzs/nina/blob/master/examples)，结合注释来学习哦！

# Nina的VSCode扩展
Nina的语法高亮扩展在VSCode扩展商城上线了哦！直接搜“Nina”就有了！[源码 github 地址](https://github.com/jwhgzs/nina-extension)~

虽然只是一个玩乐项目，但是加上扩展玩起来也开心一些呢~