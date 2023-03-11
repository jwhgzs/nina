# 关于作者

这是我（作者本人）信息：

- **姓名：** 谭镇洋
- **学校：** 台山市新宁中学
- **班别：** 2020秋11班（2023年届初三毕业生）
- **个人网站：** [jwhgzs.com](https://jwhgzs.com)

# 环境要求

- Windows 10 及以上

# 使用方法

Nina 内置了简单易用的 CLI （命令行工具）。安装 Nina 后，运行 Nina 主程序（例如通过快捷方式），即可使用 Nina 内置的 CLI。

自己试试就知道 Nina 的 CLI 有多爽啦！

# 初识 Nina

这是一个全新的脚本编程语言，简单、易学、语义化，拥有独创的模块化语法控制。

要搞明白上面的表述是什么意思，请先来看这么一段 Nina 代码：

```
应用 "中文", "强类型";

输出("请输入表达式：");
变量 表达式 = 输入();
变量 代码 = "应用 '中文'; 返回 " + 表达式;
变量 结果;
尝试 {
    结果 = 执行(代码, 空);
    如果 (结果 == 空) {
        抛出("");
    }
}
捕捉 {
    输出换行("计算失败！");
    退出();
}

输出换行("计算结果：" + 到字符串(结果));
```

这就是用 Nina 写的一段实现简易计算器的代码。是不是很惊讶？没错，得益于模块化语法控制的加持， Nina 原生地支持中文编程，你还可以无缝地切换语法特性。

这样看来， Nina 似乎很适合刚学习编程的菜鸟呢！没错——这就是 Nina 的最大卖点之一——简单、易学：

- **简易的、无歧义的语法。** 结合了各个语言的语法优点，取其精华去其糟粕，对菜鸟更加友好
- **模块化语法控制。** 因此可支持中文编程，降低学习门槛；切换语法特性，在同一个语言平台上，学习不同语言的特点

下面将会介绍 Nina 中所有的语法特性~

# Nina 的基本语法

这里将通过几个实例来介绍 Nina 的基本语法：

（另：为了方便，这里不采用中文编程的语法模块）

### 变量与常量

```
// 通过 var 关键字定义变量。可以如下进行定义时初始化，当然也可以选择不初始化
// 若变量或常量未初始化，则会被赋初始值 null （空）
var a = 1, b = 2, c = 3;
var d, e, f;
// 变量的使用。
a = a + 1;
d = b * 2 + 1;

// 通过 const 关键字定义常量。常量的定义也不要求一定要初始化，但是一般来说都需要初始化。
const pi = 3.14;
const my_const;
// 若解开注释，下面这行会报错，因为常量定义后不能重新赋值
// pi = 3, my_const = 2023;
// 常量除了不能定义后修改，其他特性与变量一样！

// 另外，允许重定义变量，即重名变量。例如下面这行不会报错。
var a = 2023;
```

### 运算符

```
var a = 7, b = 4;
var obj = object {
    var y = 2022;
    func f() {
        return this.y + 1;
    }
};
var f = obj.f;

/* 双目运算符 */
// 基本数学运算
console_printf(a + b); // 加
console_printf(a - b); // 减
console_printf(a * b); // 乘
console_printf(a / b); // 除
console_printf(a % b); // 求余
console_printf(a ** b); // 幂
// 位运算
console_printf(a & b); // 按位与
console_printf(a | b); // 按位或
console_printf(a ^ b); // 按位异或
// 逻辑运算（短路）
console_printf(a && b); // 逻辑与
console_printf(a ? b); // 逻辑与（语法糖）
console_printf(a || b); // 逻辑或
console_printf(a : b); // 逻辑或（语法糖）
console_printf(1 ? a : b); // 逻辑与 + 逻辑或，等价于其它语言的三目运算符
// 逻辑运算（普通）
console_printf(a > b); // 大于
console_printf(a < b); // 小于
console_printf(a >= b); // 大于等于
console_printf(a <= b); // 小于等于
console_printf(a == b); // 等于
console_printf(a != b); // 不等于
// 特殊运算
console_printf(a = 24); // 赋值，运算结果（运算值）为被赋值变量的新值
console_printf(obj["y"]); // 成员访问，后面“对象和数组”部分会详细讲解
console_printf(obj.y); // 成员访问，后面“对象和数组”部分会详细讲解
console_printf(obj.f()); // 函数调用
var obj2 = object {}, arr = array (); // 对象、数组创建运算符，后面“对象和数组”部分会详细讲解

/* 单目运算符 */
// 基本数学运算
console_printf(+ a); // 正
console_printf(- a); // 负
// 位运算
console_printf(~ a); // 按位取反
// 逻辑运算
console_printf(! a); // 逻辑非
// 特殊运算
console_printf(typeof a); // 类型获取运算符，运算值形如 [Type]
console_printf(f(@ obj)); // 隐藏参数修饰符，后面“函数”部分会详细讲解
```

### 对象与数组

```
// 通过 object 运算符创建对象。其右手值为内联初始化语句块。
var obj = object {
    // 通过变量、常量、函数定义语句来语义化地定义对象成员：
    var a = 1;
    const b = 2;
    // 通过定义特殊变量或常量 type 来修改类型名
    const type = "MyType";
    func c() {
        // this 指代当前函数执行的上下文，下文“函数”部分将详解
        return this.a + this.b;
    }
};
obj.a = 3;
// 解开注释后下面这行会报错，因为常量成员定义后不能被重新赋值
// obj.b = 4;
// 通过成员访问运算符 . 访问对象的成员
console_printf(obj.c());
// 将输出 5
// 通过成员访问运算符 [ 访问对象的成员
console_printf(obj["c"]());
// 也将输出 5
// 注意！ Nina 中对对象、数组中未定义成员的访问、赋值不受限制，例如下面这几行不会报错：
obj.d = 2022;
console_printf(obj.e);
// 将输出 [Null] ，即空
console_printf(typeof obj);
// 将输出 [Object: MyType]

// 通过 array 运算符创建数组。其右手值为列表表达式。
var arr = array (2020, 2021, 2022, 2023);
console_printf(arr[3]);
// 将输出 2023
```

### 逻辑控制

```
var a = 2023;
// 简单的 if - elseif - else 语句。
// 注意， Nina 会对 if 和 else 等语句的条件表达式进行自动类型转换，转为布尔类型再进行判断、分支
if (a) {
    console_printf("a is truethy.");
}
elseif (a == 2023) {
    console_printf("a is 2023.");
}
else {
    console_printf("a is unknown.");
}
// 将输出： a is truethy.

// Nina 中只有 while 一种循环语句。
var i = - 1;
while ((i = i + 1) < 5) {
    if (i + 1 == 2) {
        // continue 语句表示直接跳到下一次循环
        continue;
    }
    console_printf(i + 1);
    if (i + 1 == 4) {
        // break 语句表示退出当前整一个循环
        break;
    }
}
// 这里将会依次输出 1 3 4
```

### 函数

```
// 通过 func 语法糖定义函数。函数形参列表支持初始值。
// 注意！函数形参初始值会在每次调用时计算！
func f(a, b, c = random_range(1, 100)) {
    // 通过 return 语句返回函数值或结束函数。
    // return 语句支持省略表达式，此时函数值将设置为 null （空）
    return a + b * c;
}
console_printf(f(3, 4));
console_printf(f(3, 4));
console_printf(f(3, 4));
// 将输出三个不同的数

// Nina 完全支持闭包。实际上， Nina 中所有函数都是闭包。
// 例如上面的例子可以用 lambda 表达式转写成这样：
var f = (a, b, c = random_range(1, 100)) => {
    return () => { return a + b * c; };
};
console_printf(f(3, 4)());
console_printf(f(3, 4)());
console_printf(f(3, 4)());
// 也是输出三个不同的数

// 另外， Nina 中函数上下文中有 self 和 this 两个特殊变量
// self 的含义很简单，即指向当前函数
func fibonacci(n) {
    if (n == 1 || n == 2) {
        return 1;
    }
    return self(n - 1) + self(n - 2);
}
console_printf(fibonacci(33));
// 将输出 3524578
// 相比之下， this 的含义就有些难理解。
// 从定义来说，作为对象或数组成员的函数，其特殊变量 this 指向函数所在的上下文（即其所在的对象或数组）：
var obj = object {
    var y = 2022;
    func f() {
        return this.y + 1;
    }
};
console_printf(obj.f());
// 将输出 2023
var f = obj.f;
// 解开下面这行注释后，将报错
// 因为此时调用的函数 f 脱离了其所在对象，不再有有效的 this 上下文（此时 this 的值为 null ，即空）
// console_printf(f());
// 使用隐藏参数修饰符 @ ，可以修改 this 的指向。因此下面这行能正常输出
console_printf(f(@ obj));
// 将输出 2023
```

### 异常处理

```
// 使用 try - catch 语句进行异常捕获。
// 注意！编译时的致命错误（语法错误等）不能被 try - catch 捕获。
try {
    var a = null[0];
}
catch {
    console_printf("");
    console_printf("----- 捕获到异常：");
    // 通过特殊变量 exception 获取捕获到的异常信息
    console_printf("错误代号：" + exception.code);
    console_printf("错误信息：" + exception.message);
    console_printf("");
}

// 使用内置函数 throw(message) 来抛出自定义异常
try {
    throw("这是自定义的异常哦！~");
}
catch {
    console_printf("");
    console_printf("----- 捕获到异常：");
    console_printf("错误代号：" + exception.code);
    console_printf("错误信息：" + exception.message);
    console_printf("");
}
```

### 动态运行

动态运行作为脚本语言的“必备技能”， Nina 也不例外~

```
// 通过 eval(code, argument) 动态执行代码，并传递参数
console_printf(
    eval(
        "
            // 通过特殊变量 argument 获取从上端传递来的数据
            // 用顶级的 return 语句向上端返回值
            return argument + 1;
        ",
        // 第二个参数是要传递给动态代码的数据
        2022
    )
);
// 将输出 2023
```

### 模块化语法控制

这是 Nina 最大的特色——模块化语法控制。有了它，你可以轻易地转换语法和特性，体验不同的语言风格。

#### 强类型模式

Nina 默认是弱类型的。不过你可以通过 with 语句进行模块化语法控制，改变这个特性。

```
// 使用 with 语句控制语法模块，启用强类型模式
// 注意！ with 语句只能出现在第一句。
with "strongly";
// 此时若解开下面一行的注释，将会报错，因为启用了强类型语法模块， Nina 将不允许进行隐式类型转换
// console_printf("2" + 4);
// 利用 number(data) 和 string(data) 来进行显示类型转换
console_printf("2" + string(4));
console_printf(number("2") + 4);
// 分别输出： 24 6

// 不过有一点需要注意，无论在强类型还是弱类型模式，除了 console_print 等特殊函数， Nina 的内置函数都是强类型的。
// 解开下面一行的注释将会报错（无论开启或关闭强类型模式）
// 因为调用内置函数时 Nina 不允许进行隐式类型转换（即此时为强类型模式）
// console_printf(math_log("2", 10));
```

#### 中文模式

Nina 默认不支持中文作标识符名，也没有中文关键字的支持。你可以通过 with 语句启用它们。

```
// 使用 `应用` 语句（等价于 with 语句）启用中文模式
// 为了不出现“中英混杂”， Nina 允许 `应用` 语句在未启用中文模式时也可以使用
// 另外，语法模块名也默认有中英两个版本可以使用：
// 下面这行等价于： with "chinese", "strongly";
应用 "中文", "强类型";
// 终于支持了中文标识符！
函数 初始函数() {
    // 此时内置函数也做了汉化，详见下一部分的表
    输出换行("你好，世界");
}
初始函数();
// 不过，启用中文模式后原有的关键字、运算符、内置函数等仍能正常使用！
```

### 内置中文模式翻译表

由于时间问题，内置常量、函数的解释暂时不做，请先望文生义、将就用一下哦~

另外，下表标记“中文”不是单纯的翻译哦，是启用了中文语法模块后（下文详解）对应的函数名称！

#### 关键字

- `var` ，中文 `变量`
- `const` ，中文 `常量`
- `func` ，中文 `函数`
- `if` ，中文 `如果`
- `else` ，中文 `否则`
- `elseif` ，中文 `否则如果`
- `while` ，中文 `条件循环`
- `return` ，中文 `返回`
- `break` ，中文 `退出循环`
- `continue` ，中文 `继续循环`
- `try` ，中文 `尝试`
- `catch` ，中文 `捕捉`
- `with` ，中文 `应用`

#### 常量

- `null` ，中文 `空`
- `true` ，中文 `真`
- `false` ，中文 `假`
- `math_PI` ，中文 `数学_PI`
- `math_E` ，中文 `数学_E`

#### 函数

- `number` ，中文 `到数字`
- `string` ，中文 `到字符串`
- `eval` ，中文 `执行`
- `throw` ，中文 `抛出`
- `console_print` ，中文 `输出`
- `console_printf` ，中文 `输出换行`
- `console_read` ，中文 `输入`
- `console_exit` ，中文 `退出`
- `string_at` ，中文 `字符串_取字符`
- `string_sub` ，中文 `字符串_截取`
- `string_sub_to_tail` ，中文 `字符串_截取到末尾`
- `string_split` ，中文 `字符串_分割`
- `string_split_count` ，中文 `字符串_按次数分割`
- `string_to_array` ，中文 `字符串_字符串到数组`
- `string_from_array` ，中文 `字符串_数组到字符串`
- `string_from_array_join` ，中文 `字符串_连接数组到字符串`
- `string_replace` ，中文 `字符串_替换`
- `string_replace_count` ，中文 `字符串_按次数替换`
- `string_upper` ，中文 `字符串_到大写`
- `string_lower` ，中文 `字符串_到小写`
- `string_length` ，中文 `字符串_取长度`
- `array_length` ，中文 `数组_取长度`
- `array_append` ，中文 `数组_添加项`
- `array_insert` ，中文 `数组_插入项`
- `array_pop` ，中文 `数组_移除末尾项`
- `array_remove` ，中文 `数组_移除项`
- `array_clear` ，中文 `数组_清空`
- `array_find` ，中文 `数组_查找值`
- `array_find_last` ，中文 `数组_反向查找值`
- `array_to_JSON` ，中文 `数组_数组到JSON`
- `array_from_JSON` ，中文 `数组_JSON到数组`
- `object_length` ，中文 `对象_取长度`
- `object_has` ，中文 `对象_是否存在键`
- `object_find` ，中文 `对象_查找值`
- `object_find_last` ，中文 `对象_反向查找值`
- `object_remove` ，中文 `对象_移除项`
- `object_clear` ，中文 `对象_清空`
- `object_to_JSON` ，中文 `对象_对象到JSON`
- `object_from_JSON` ，中文 `对象_JSON到对象`
- `time_now` ，中文 `时间_取当前时间戳`
- `time_to_string` ，中文 `时间_时间戳到字符串`
- `time_to_object` ，中文 `时间_时间戳到时间对象`
- `time_from_string` ，中文 `时间_字符串到时间戳`
- `random_raw` ，中文 `随机数_原始值`
- `random_range` ，中文 `随机数_有范围`
- `math_floor` ，中文 `数学_向下舍入`
- `math_ceil` ，中文 `数学_向上舍入`
- `math_round` ，中文 `数学_四舍五入`
- `math_round_digit` ，中文 `数学_按位数四舍五入`
- `math_sin` ，中文 `数学_正弦`
- `math_cos` ，中文 `数学_余弦`
- `math_tan` ，中文 `数学_正切`
- `math_asin` ，中文 `数学_反正弦`
- `math_acos` ，中文 `数学_反余弦`
- `math_atan` ，中文 `数学_反正切`
- `math_sqrt` ，中文 `数学_平方根`
- `math_abs` ，中文 `数学_绝对值`
- `math_max` ，中文 `数学_最大值`
- `math_min` ，中文 `数学_最小值`
- `math_log` ，中文 `数学_对数`
- `fs_file_create` ，中文 `文件系统_文件_创建`
- `fs_file_delete` ，中文 `文件系统_文件_删除`
- `fs_file_read` ，中文 `文件系统_文件_读取`
- `fs_file_write` ，中文 `文件系统_文件_写入`
- `fs_file_has` ，中文 `文件系统_文件_是否存在`
- `fs_file_move` ，中文 `文件系统_文件_移动`
- `fs_file_info` ，中文 `文件系统_文件_取信息`
- `fs_dir_create` ，中文 `文件系统_目录_创建`
- `fs_dir_delete` ，中文 `文件系统_目录_删除`
- `fs_dir_read` ，中文 `文件系统_目录_读取`
- `fs_dir_move` ，中文 `文件系统_目录_移动`
- `fs_dir_info` ，中文 `文件系统_目录_取信息`

#### 特殊变量

- `self` ，中文 `自身函数`
- `this` ，中文 `自身对象`
- `argument` ，中文 `参数`
- `exception` ，中文 `异常`

#### 运算符

- `typeof` ，中文 `取类型`
- `object` ，中文 `新对象`
- `array` ，中文 `新数组`