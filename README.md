# Nina的自我介绍
Hello! 我就是实习时长两周半的实习生——Nina！
这是我的简历：
- 姓名： Nina Script （尼娜·斯克里比特）
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
};

// 保存当前时间戳以计时
var t1 = time_now();
// 输出结果和耗时
console_printf('fibo(33) = ' + fibo(33));
console_printf('time: ' + (time_now() - t1) * 1000 + ' ms');
```
这是一段计算斐波那契数的Nina代码。事实上，如果把关键字`func`改为`function`，这就是一段正经的的JavaScript代码！
从中还可以看出，Nina的两个基本属性：
- 动态类型语言
- 弱类型语言

这一点也与JavaScript完全一致！看来这对姐妹花是亲生的没错了。

# 进一步了解Nina
由于Nina懒得写更多的自我介绍，你可以翻看项目文件中的[examples](https://github.com/jwhgzs/Nina/blob/master/examples)，结合注释来学习哦！